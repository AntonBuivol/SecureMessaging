using Microsoft.Maui.Storage;

namespace SecureMessaging.Services;

public class SettingsService
{
    private const string UserIdKey = "user_id";
    private const string DeviceIdKey = "device_id";
    private const string ThemeKey = "app_theme";
    private const string FontSizeKey = "font_size";
    private const int DefaultFontSize = 14;

    // Сохранение сессии пользователя
    public async Task SaveUserSession(string userId, string deviceId)
    {
        try
        {
            await SecureStorage.SetAsync(UserIdKey, userId);
            await SecureStorage.SetAsync(DeviceIdKey, deviceId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving session: {ex.Message}");
        }
    }

    // Получение сессии
    public async Task<(string UserId, string DeviceId)> GetUserSession()
    {
        try
        {
            var userId = await SecureStorage.GetAsync(UserIdKey);
            var deviceId = await SecureStorage.GetAsync(DeviceIdKey);
            return (userId, deviceId);
        }
        catch
        {
            return (null, null);
        }
    }

    // Очистка сессии
    public async Task ClearUserSession()
    {
        try
        {
            SecureStorage.Remove(UserIdKey);
            SecureStorage.Remove(DeviceIdKey);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing session: {ex.Message}");
        }
    }

    // Настройки шрифта
    public void SetFontSize(int size)
    {
        try
        {
            Preferences.Set(FontSizeKey, size);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving font size: {ex.Message}");
        }
    }

    public int GetFontSize()
    {
        try
        {
            return Preferences.Get(FontSizeKey, DefaultFontSize);
        }
        catch
        {
            return DefaultFontSize;
        }
    }

    // Настройки темы
    public void SetTheme(bool isDark)
    {
        try
        {
            Preferences.Set(ThemeKey, isDark ? "Dark" : "Light");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving theme: {ex.Message}");
        }
    }

    public string GetTheme()
    {
        try
        {
            return Preferences.Get(ThemeKey, "Light");
        }
        catch
        {
            return "Light";
        }
    }
}