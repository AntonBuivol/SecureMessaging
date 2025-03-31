using SecureMessaging.Models;
using SecureMessaging.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SecureMessaging.Views;

public partial class ChatListPage : ContentPage
{
    private readonly ChatService _chatService;
    private readonly AuthService _authService;
    private readonly SearchService _searchService;

    private List<Chat> _allChats = new();
    private string _currentUserId;

    public ChatListPage(ChatService chatService, AuthService authService, SearchService searchService)
    {
        InitializeComponent();
        _chatService = chatService;
        _authService = authService;
        _searchService = searchService;
        BindingContext = this;

        LoadChats();
    }

    public ObservableCollection<Chat> DisplayItems { get; } = new();
    public ObservableCollection<User> SearchResults { get; } = new();

    public bool IsRefreshing { get; set; }
    public bool IsSearchVisible { get; set; }
    public bool IsSearching => IsSearchVisible && !string.IsNullOrEmpty(SearchQuery);
    public string SearchQuery { get; set; }

    public ICommand RefreshCommand => new Command(async () =>
    {
        IsRefreshing = true;
        OnPropertyChanged(nameof(IsRefreshing));

        await LoadChats();

        IsRefreshing = false;
        OnPropertyChanged(nameof(IsRefreshing));
    });

    public ICommand ToggleSearchCommand => new Command(() =>
    {
        IsSearchVisible = !IsSearchVisible;
        OnPropertyChanged(nameof(IsSearchVisible));
        OnPropertyChanged(nameof(IsSearching));

        if (!IsSearchVisible)
        {
            SearchQuery = string.Empty;
            OnPropertyChanged(nameof(SearchQuery));
            SearchResults.Clear();
        }
    });

    public ICommand SearchCommand => new Command(async () =>
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            SearchResults.Clear();
            return;
        }

        var results = await _searchService.SearchUsers(SearchQuery);
        SearchResults.Clear();

        foreach (var user in results.Where(u => u.Id != _currentUserId))
        {
            SearchResults.Add(user);
        }
    });

    public ICommand OpenChatWithUserCommand => new Command<User>(async (user) =>
    {
        if (user == null) return;

        var chat = await _searchService.StartDirectMessage(_currentUserId, user.Id, user.Username, user.DisplayName);

        // Add to chats if not already there
        if (!_allChats.Any(c => c.Id == chat.Id))
        {
            _allChats.Add(chat);
            DisplayItems.Add(chat);
        }

        // Navigate to chat page (you'll need to implement this)
        // await Shell.Current.GoToAsync($"ChatPage?chatId={chat.Id}");

        // Reset search
        IsSearchVisible = false;
        OnPropertyChanged(nameof(IsSearchVisible));
        OnPropertyChanged(nameof(IsSearching));
        SearchQuery = string.Empty;
        OnPropertyChanged(nameof(SearchQuery));
        SearchResults.Clear();
    });

    public ICommand OpenMenuCommand => new Command(async () =>
    {
        var action = await DisplayActionSheet("Menu", "Cancel", null,
            "Profile Settings", "App Settings", "Logout");

        switch (action)
        {
            case "Profile Settings":
                await Shell.Current.GoToAsync("//ProfileSettingsPage");
                break;
            case "App Settings":
                await Shell.Current.GoToAsync("//AppSettingsPage");
                break;
            case "Logout":
                await _authService.Logout();
                await Shell.Current.GoToAsync("//MainPage");
                break;
        }
    });

    private async Task LoadChats()
    {
        var (userId, _) = await _authService.GetUserSession();
        _currentUserId = userId;

        var chats = await _chatService.GetUserChats(userId);
        _allChats = chats;

        DisplayItems.Clear();
        foreach (var chat in chats)
        {
            DisplayItems.Add(chat);
        }
    }
}