using SecureMessaging.Models;
using SecureMessaging.Services;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Maui.Storage;

namespace SecureMessaging.Services;

public class AuthService
{
    private const string UserIdKey = "user_id";
    private const string DeviceIdKey = "device_id";

    private readonly SupabaseService _supabase;
    private readonly ChatService _chatService;
    private readonly IServiceProvider _serviceProvider;

    public AuthService(SupabaseService supabase, IServiceProvider serviceProvider)
    {
        _supabase = supabase;
        _serviceProvider = serviceProvider;
    }

    public async Task<bool> IsLoggedIn()
    {
        try
        {
            var userId = await SecureStorage.Default.GetAsync(UserIdKey);
            var deviceId = await SecureStorage.Default.GetAsync(DeviceIdKey);

            if (string.IsNullOrEmpty(userId))
                return false;

            var device = await _supabase.Client
                .From<UserDeviceInfo>()
                .Where(x => x.Id == deviceId && x.UserId == userId && x.IsCurrent)
                .Single();

            return device != null;
        }
        catch
        {
            await ClearUserSession();
            return false;
        }
    }

    public async Task<(string UserId, string DeviceId)> GetUserSession()
    {
        try
        {
            var userId = await SecureStorage.Default.GetAsync(UserIdKey);
            var deviceId = await SecureStorage.Default.GetAsync(DeviceIdKey);
            return (userId, deviceId);
        }
        catch
        {
            return (null, null);
        }
    }

    public async Task<bool> Register(string username, string password, string displayName = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            if (await _supabase.CheckUsernameExists(username))
                return false;

            var passwordHash = HashPassword(password);

            var user = new User
            {
                Username = username,
                PasswordHash = passwordHash,
                DisplayName = displayName ?? username
            };

            // 1. Создаем пользователя
            var response = await _supabase.Client.From<User>().Insert(user);
            var createdUser = response.Model;

            // 2. Создаем запись об устройстве
            var deviceInfo = await GetDeviceInfo();
            deviceInfo.UserId = createdUser.Id;
            await _supabase.Client.From<UserDeviceInfo>().Insert(deviceInfo);
            await _supabase.SetCurrentDevice(createdUser.Id, deviceInfo.Id);

            // 3. Создаем чаты с существующими пользователями
            var allUsers = await _supabase.GetAllUsersExceptCurrent(createdUser.Id);
            foreach (var existingUser in allUsers)
            {
                // Создаем direct-чат между новым пользователем и существующим
                await _chatService.GetOrCreateDirectChat(createdUser.Id, existingUser.Id);
            }

            // Сохраняем сессию
            await SecureStorage.Default.SetAsync(UserIdKey, createdUser.Id);
            await SecureStorage.Default.SetAsync(DeviceIdKey, deviceInfo.Id);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Registration error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Login(string username, string password)
    {
        try
        {
            var user = await _supabase.Client
                .From<User>()
                .Where(x => x.Username == username)
                .Single();

            if (user == null) return false;

            if (user.PasswordHash != HashPassword(password))
                return false;

            var deviceInfo = await GetDeviceInfo();
            deviceInfo.UserId = user.Id;
            deviceInfo.IsCurrent = true;
            deviceInfo.LastLogin = DateTime.UtcNow;

            // Деактивируем все другие устройства этого пользователя
            await _supabase.Client
                .From<UserDeviceInfo>()
                .Where(x => x.UserId == user.Id)
                .Set(x => x.IsCurrent, false)
                .Update();

            // Если устройство новое - сохраняем, иначе обновляем
            if (string.IsNullOrEmpty(deviceInfo.Id))
            {
                var response = await _supabase.Client.From<UserDeviceInfo>().Insert(deviceInfo);
                deviceInfo = response.Model;
            }
            else
            {
                await _supabase.Client.From<UserDeviceInfo>()
                    .Where(x => x.Id == deviceInfo.Id)
                    .Set(x => x.IsCurrent, true)
                    .Set(x => x.LastLogin, DateTime.UtcNow)
                    .Update();
            }

            // Сохраняем сессию
            await SecureStorage.Default.SetAsync(UserIdKey, user.Id);
            await SecureStorage.Default.SetAsync(DeviceIdKey, deviceInfo.Id);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login error: {ex.Message}");
            await ClearUserSession();
            return false;
        }
    }

    public async Task Logout()
    {
        try
        {
            var (userId, deviceId) = await GetUserSession();
            if (!string.IsNullOrEmpty(deviceId))
            {
                await _supabase.Client
                    .From<UserDeviceInfo>()
                    .Where(x => x.Id == deviceId)
                    .Set(x => x.IsCurrent, false)
                    .Update();
            }
        }
        finally
        {
            await ClearUserSession();
        }
    }

    private async Task ClearUserSession()
    {
        try
        {
            SecureStorage.Default.Remove(UserIdKey);
            SecureStorage.Default.Remove(DeviceIdKey);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ClearUserSession error: {ex.Message}");
        }
    }

    private async Task<UserDeviceInfo> GetDeviceInfo()
    {
        var deviceId = await GetDeviceId();

        try
        {
            var existingDevice = await _supabase.Client
                .From<UserDeviceInfo>()
                .Where(x => x.DeviceId == deviceId)
                .Single();

            if (existingDevice != null)
                return existingDevice;
        }
        catch
        {
            // Устройство не найдено - создаем новое
        }

        return new UserDeviceInfo
        {
            DeviceId = deviceId,
            DeviceName = DeviceInfo.Name,
            DeviceModel = DeviceInfo.Model,
            OsVersion = DeviceInfo.VersionString,
            LastLogin = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    private async Task<string> GetDeviceId()
    {
        // Генерируем уникальный ID устройства на основе аппаратных характеристик
        var deviceId = $"{DeviceInfo.Manufacturer}_{DeviceInfo.Model}_{DeviceInfo.Name}";
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(deviceId));
        return Convert.ToBase64String(hash);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}