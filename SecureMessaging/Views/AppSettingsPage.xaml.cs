using SecureMessaging.Models;
using SecureMessaging.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SecureMessaging.Views;

public partial class AppSettingsPage : ContentPage
{
    private readonly SupabaseService _supabase;
    private readonly AuthService _authService;

    public AppSettingsPage(SupabaseService supabase, AuthService authService)
    {
        InitializeComponent();
        _supabase = supabase;
        _authService = authService;
        BindingContext = this;

        LoadDevices();
    }

    public UserDeviceInfo CurrentDevice { get; private set; }
    public ObservableCollection<UserDeviceInfo> OtherDevices { get; } = new();

    private async void LoadDevices()
    {
        var devices = await _supabase.Client.From<UserDeviceInfo>()
            .Where(x => x.UserId == _authService.CurrentUser.Id)
            .Get();

        CurrentDevice = devices.Models.FirstOrDefault(d => d.IsCurrent);
        OtherDevices.Clear();

        foreach (var device in devices.Models.Where(d => !d.IsCurrent))
        {
            OtherDevices.Add(device);
        }
    }

    public ICommand LogoutCommand => new Command(async () =>
    {
        await _authService.Logout();
        await Shell.Current.GoToAsync("//MainPage");
    });
}