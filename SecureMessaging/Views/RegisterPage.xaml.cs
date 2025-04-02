using SecureMessaging.Services;

namespace SecureMessaging.Views.Auth;

public partial class RegisterPage : ContentPage
{
    private readonly AuthService _authService;

    public RegisterPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
        BindingContext = this;
    }

    public string Username { get; set; }
    public string Password { get; set; }
    public string DisplayName { get; set; }

    public Command RegisterCommand => new Command(async () =>
    {
        if (await _authService.Register(Username, Password, DisplayName))
        {
            await Shell.Current.GoToAsync("///ChatListPage");
        }
        else
        {
            await DisplayAlert("Error", "Registration failed", "OK");
        }
    });

    public Command GoToLoginCommand => new Command(async () =>
    {
        await Shell.Current.GoToAsync("///LoginPage");
    });
}