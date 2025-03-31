using SecureMessaging.Models;
using SecureMessaging.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SecureMessaging.Views;

public partial class ChatPage : ContentPage
{
    private readonly ChatService _chatService;
    private readonly AuthService _authService;
    private readonly UserService _userService;
    private string _currentUserId;

    public ObservableCollection<User> Users { get; } = new();
    public ObservableCollection<User> FilteredUsers { get; } = new();

    public ChatPage(ChatService chatService, AuthService authService, UserService userService)
    {
        InitializeComponent();
        _chatService = chatService;
        _authService = authService;
        _userService = userService;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var (userId, _) = await _authService.GetUserSession();
        _currentUserId = userId;
        await LoadAllUsers();
    }

    private async Task LoadAllUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersExceptCurrent(_currentUserId);
            Users.Clear();

            foreach (var user in users)
            {
                Users.Add(user);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load users: {ex.Message}", "OK");
        }
    }

    public ICommand StartChatCommand => new Command<User>(async (user) =>
    {
        if (user == null) return;

        try
        {
            // Создаем или получаем существующий чат
            var chat = await _chatService.GetOrCreateDirectChat(_currentUserId, user.Id);

            // Открываем страницу сообщений
            await Shell.Current.GoToAsync($"MessagePage?chatId={chat.Id}");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to start chat: {ex.Message}", "OK");
        }
    });

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = e.NewTextValue?.ToLower() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(searchText))
        {
            FilteredUsers.Clear();
            return;
        }

        FilteredUsers.Clear();
        foreach (var user in Users.Where(u =>
            u.Username.ToLower().Contains(searchText) ||
            (u.DisplayName?.ToLower()?.Contains(searchText) ?? false)))
        {
            FilteredUsers.Add(user);
        }
    }
}