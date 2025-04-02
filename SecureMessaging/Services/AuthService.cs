using SecureMessaging.Models;
using SecureMessaging.Services;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Maui.Storage;

namespace SecureMessaging.Services;

public class AuthService
{
    private readonly SupabaseService _supabase;
    private User _currentUser;

    public AuthService(SupabaseService supabase)
    {
        _supabase = supabase;
    }

    public User CurrentUser => _currentUser;

    public async Task<bool> Register(string username, string password, string displayName = null)
    {
        if (await _supabase.Client.From<User>()
            .Where(x => x.Username == username)
            .Single() != null)
            return false;

        var user = new User
        {
            Username = username,
            PasswordHash = HashPassword(password),
            DisplayName = displayName ?? username
        };

        var response = await _supabase.Client.From<User>().Insert(user);
        _currentUser = response.Model;

        await RegisterDevice(_currentUser.Id);
        await InitializeUserChats(_currentUser.Id);

        return true;
    }

    public async Task<bool> Login(string username, string password)
    {
        var user = await _supabase.Client.From<User>()
            .Where(x => x.Username == username)
            .Single();

        if (user?.PasswordHash != HashPassword(password))
            return false;

        _currentUser = user;
        await RegisterDevice(_currentUser.Id);
        return true;
    }


    public async Task Logout()
    {
        if (_currentUser == null) return;

        var deviceId = GetDeviceUniqueId();

        try
        {
            // Удаляем текущее устройство
            await _supabase.Client.From<UserDeviceInfo>()
                .Where(x => x.UserId == _currentUser.Id && x.DeviceId == deviceId)
                .Delete();

            _currentUser = null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during logout: {ex.Message}");
            // Можно добавить обработку ошибки, если нужно
        }
    }

    private async Task RegisterDevice(string userId)
    {
        var deviceInfo = new UserDeviceInfo
        {
            UserId = userId,
            DeviceName = $"{DeviceInfo.Manufacturer} {DeviceInfo.Model}",
            DeviceId = GetDeviceUniqueId(),
            IsCurrent = true,
            LastLogin = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // Деактивируем все другие устройства пользователя
        await _supabase.Client.From<UserDeviceInfo>()
            .Where(x => x.UserId == userId)
            .Set(x => x.IsCurrent, false)
            .Update();

        // Проверяем существование устройства
        var existingDevice = await _supabase.Client.From<UserDeviceInfo>()
            .Where(x => x.UserId == userId && x.DeviceId == deviceInfo.DeviceId)
            .Single();

        if (existingDevice == null)
        {
            // Новое устройство
            await _supabase.Client.From<UserDeviceInfo>().Insert(deviceInfo);
        }
        else
        {
            // Обновляем существующее устройство
            await _supabase.Client.From<UserDeviceInfo>()
                .Where(x => x.Id == existingDevice.Id)
                .Set(x => x.IsCurrent, true)
                .Set(x => x.LastLogin, DateTime.UtcNow)
                .Set(x => x.DeviceName, deviceInfo.DeviceName)
                .Update();
        }
    }

    private string GetDeviceUniqueId()
    {
        var deviceInfo = $"{DeviceInfo.Manufacturer}_{DeviceInfo.Model}_{DeviceInfo.Name}";
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(deviceInfo));
        return Convert.ToHexString(hash); // Используем HexString вместо Base64 для совместимости
    }



    private async Task InitializeUserChats(string userId)
    {
        try
        {
            // Получаем всех существующих пользователей, кроме нового
            var existingUsers = await _supabase.Client.From<User>()
                .Where(x => x.Id != userId)
                .Get();

            // Для каждого существующего пользователя создаем чат
            foreach (var user in existingUsers.Models)
            {
                await CreateDirectChat(userId, user.Id);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing chats: {ex.Message}");
        }
    }

    private async Task CreateDirectChat(string userId1, string userId2)
    {
        // Получаем данные второго пользователя
        var user2 = await _supabase.Client.From<User>()
            .Where(x => x.Id == userId2)
            .Single();

        // Создаем новый чат
        var newChat = new Chat
        {
            IsDirectMessage = true,
            ChatName = user2.DisplayName ?? user2.Username,
            CreatedAt = DateTime.UtcNow
        };

        var chatResponse = await _supabase.Client.From<Chat>().Insert(newChat);

        // Добавляем участников
        await _supabase.Client.From<ChatParticipant>().Insert(new[]
        {
        new ChatParticipant
        {
            ChatId = chatResponse.Model.Id,
            UserId = userId1,
            CreatedAt = DateTime.UtcNow
        },
        new ChatParticipant
        {
            ChatId = chatResponse.Model.Id,
            UserId = userId2,
            CreatedAt = DateTime.UtcNow
        }
    });
    }



    private string GetDeviceId()
    {
        return $"{DeviceInfo.Manufacturer}_{DeviceInfo.Model}";
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}