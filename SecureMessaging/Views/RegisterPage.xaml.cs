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
    public string ConfirmPassword { get; set; }
    public string DisplayName { get; set; }

    public Command RegisterCommand => new Command(async () =>
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            await DisplayAlert("Error", "Username and password are required", "OK");
            return;
        }

        if (Password != ConfirmPassword)
        {
            await DisplayAlert("Error", "Passwords don't match", "OK");
            return;
        }

        var success = await _authService.Register(Username, Password, DisplayName);
        if (success)
        {
            await Shell.Current.GoToAsync("///ChatListPage");
        }
        else
        {
            await DisplayAlert("Error", "Registration failed. Username may already exist.", "OK");
        }
    });

    public Command GoToLoginCommand => new Command(async () =>
        await Shell.Current.GoToAsync("///LoginPage"));
}