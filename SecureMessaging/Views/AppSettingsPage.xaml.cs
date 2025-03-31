using SecureMessaging.Models;
using SecureMessaging.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SecureMessaging.Views;

public partial class AppSettingsPage : ContentPage
{
    private readonly SupabaseService _supabase;
    private readonly AuthService _authService;
    private readonly SettingsService _settingsService;

    public AppSettingsPage(SupabaseService supabase,
                         AuthService authService,
                         SettingsService settingsService)
    {
        InitializeComponent();
        _supabase = supabase;
        _authService = authService;
        _settingsService = settingsService;
        BindingContext = this;

        LoadDevices();
        LoadSettings();
    }

    // Properties
    public UserDeviceInfo CurrentDevice { get; private set; }
    public ObservableCollection<UserDeviceInfo> OtherDevices { get; } = new();

    public int FontSize
    {
        get => _settingsService.GetFontSize();
        set
        {
            _settingsService.SetFontSize(value);
            OnPropertyChanged();
        }
    }

    public int ThemeIndex
    {
        get => _settingsService.GetTheme() == "Dark" ? 1 : 0;
        set
        {
            _settingsService.SetTheme(value == 1);
            OnPropertyChanged();
            Application.Current.UserAppTheme = value == 1 ? AppTheme.Dark : AppTheme.Light;
        }
    }

    // Commands
    public ICommand GoBackCommand => new Command(async () =>
    {
        await Shell.Current.GoToAsync("//ChatListPage");
    });

    public ICommand LogoutDeviceCommand => new Command<string>(async (deviceId) =>
    {
        bool answer = await DisplayAlert("Confirm",
                                      "Logout this device?",
                                      "Yes", "No");
        if (answer)
        {
            await HandleDeviceLogout(deviceId);
        }
    });

    public ICommand SaveSettingsCommand => new Command(() =>
    {
        DisplayAlert("Success", "Settings saved", "OK");
    });

    // Methods
    private async Task LoadDevices()
    {
        try
        {
            var (userId, currentDeviceId) = await _authService.GetUserSession();
            var devices = await _supabase.GetUserDevices(userId);

            CurrentDevice = devices.FirstOrDefault(d => d.Id == currentDeviceId);
            OtherDevices.Clear();

            foreach (var device in devices.Where(d => d.Id != currentDeviceId))
            {
                OtherDevices.Add(device);
            }

            OnPropertyChanged(nameof(CurrentDevice));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading devices: {ex.Message}");
        }
    }

    private void LoadSettings()
    {
        OnPropertyChanged(nameof(FontSize));
        OnPropertyChanged(nameof(ThemeIndex));
    }

    private async Task HandleDeviceLogout(string deviceId)
    {
        try
        {
            var (_, currentDeviceId) = await _authService.GetUserSession();

            if (deviceId == currentDeviceId)
            {
                await _authService.Logout();
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                await _supabase.RemoveDevice(deviceId);
                await LoadDevices();
                await DisplayAlert("Success", "Device logged out", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to logout device: {ex.Message}", "OK");
        }
    }
}