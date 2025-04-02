using SecureMessaging.Models;
using SecureMessaging.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SecureMessaging.Views;

public partial class ChatListPage : ContentPage
{
    private readonly ChatService _chatService;
    private readonly AuthService _authService;
    private readonly UserService _userService;

    public ObservableCollection<User> Users { get; } = new();
    public bool IsRefreshing { get; set; }
    public bool IsBusy { get; set; }

    public ChatListPage(ChatService chatService, AuthService authService, UserService userService)
    {
        InitializeComponent();
        _chatService = chatService;
        _authService = authService;
        _userService = userService;
        BindingContext = this;

        LoadUsers();
    }

    private async void LoadUsers()
    {
        try
        {
            IsBusy = true;
            OnPropertyChanged(nameof(IsBusy));

            var users = await _userService.GetAllUsersExceptCurrent(_authService.CurrentUser.Id);
            Users.Clear();

            foreach (var user in users)
            {
                Users.Add(user);
            }
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(IsBusy));
        }
    }

    public ICommand RefreshCommand => new Command(async () =>
    {
        IsRefreshing = true;
        OnPropertyChanged(nameof(IsRefreshing));

        await Task.Delay(500); // Small delay for UX
        LoadUsers();

        IsRefreshing = false;
        OnPropertyChanged(nameof(IsRefreshing));
    });

    public ICommand LogoutCommand => new Command(async () =>
    {
        try
        {
            await _authService.Logout();
            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Logout failed: {ex.Message}", "OK");
        }
    });

    public ICommand OpenChatCommand => new Command<User>(async (user) =>
    {
        if (user == null) return;

        try
        {
            var chat = await _chatService.GetOrCreateDirectChat(user.Id);
            await Shell.Current.GoToAsync($"///ChatPage?chatId={chat.Id}");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to open chat: {ex.Message}", "OK");
        }
    });


    public ICommand GoToSettingsCommand => new Command(async () =>
    {
        await Shell.Current.GoToAsync("AppSettingsPage");
    });
}