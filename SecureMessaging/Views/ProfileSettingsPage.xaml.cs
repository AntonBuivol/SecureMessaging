using SecureMessaging.Models;
using SecureMessaging.Services;
using System.Windows.Input;

namespace SecureMessaging.Views;

public partial class ProfileSettingsPage : ContentPage
{
    private readonly SupabaseService _supabase;
    private readonly AuthService _authService;

    public User User { get; private set; }

    public ProfileSettingsPage(SupabaseService supabase, AuthService authService)
    {
        InitializeComponent();
        _supabase = supabase;
        _authService = authService;
        BindingContext = this;

        LoadUser();
    }

    private async void LoadUser()
    {
        User = _authService.CurrentUser;
        OnPropertyChanged(nameof(User));
    }

    public ICommand SaveCommand => new Command(async () =>
    {
        await _supabase.Client.From<User>().Update(User);
        await DisplayAlert("Success", "Profile updated", "OK");
    });
}