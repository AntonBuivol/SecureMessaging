using SecureMessaging.Services;

namespace SecureMessaging.Views.Auth;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _authService;

    public LoginPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
        BindingContext = this;
    }

    public string Username { get; set; }
    public string Password { get; set; }

    public Command LoginCommand => new Command(async () =>
    {
        if (await _authService.Login(Username, Password))
        {
            await Shell.Current.GoToAsync("///ChatListPage");
        }
        else
        {
            await DisplayAlert("Error", "Invalid credentials", "OK");
        }
    });

    public Command GoToRegisterCommand => new Command(async () =>
    {
        await Shell.Current.GoToAsync("///RegisterPage");
    });
}