using SecureMessaging.Models;
using SecureMessaging.Services;
using System.Windows.Input;

namespace SecureMessaging.Views;

public partial class ProfileSettingsPage : ContentPage
{
    private readonly SupabaseService _supabase;
    private readonly AuthService _authService;

    public ProfileSettingsPage(SupabaseService supabase, AuthService authService)
    {
        InitializeComponent();
        _supabase = supabase;
        _authService = authService;
        BindingContext = this;

        LoadUser();
    }

    public User User { get; private set; }

    public Command ChangePhotoCommand => new Command(async () =>
    {
        try
        {
            var photo = await MediaPicker.PickPhotoAsync();
            if (photo != null)
            {
                // In a real app, you would upload this to Supabase storage
                User.AvatarUrl = photo.FullPath;
                OnPropertyChanged(nameof(User));
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    });

    public Command SaveCommand => new Command(async () =>
    {
        await _supabase.UpdateUser(User);
        await DisplayAlert("Success", "Profile updated", "OK");
    });

    private async void LoadUser()
    {
        var (userId, _) = await _authService.GetUserSession();
        User = await _supabase.GetUserById(userId);
        OnPropertyChanged(nameof(User));
    }

    public ICommand GoBackCommand => new Command(async () =>
    {
        await Shell.Current.GoToAsync("//ChatListPage");
    });
}