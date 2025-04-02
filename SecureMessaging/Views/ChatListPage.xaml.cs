using SecureMessaging.Models;
using SecureMessaging.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SecureMessaging.Views;

public partial class ChatListPage : ContentPage
{
    private readonly ChatService _chatService;
    private readonly AuthService _authService;
    private readonly SupabaseService _supabase;

    public ObservableCollection<Chat> Chats { get; } = new();
    public ObservableCollection<User> SearchResults { get; } = new();
    public bool IsSearching { get; set; }

    public ChatListPage(ChatService chatService, AuthService authService, SupabaseService supabase)
    {
        InitializeComponent();
        _chatService = chatService;
        _authService = authService;
        _supabase = supabase;
        BindingContext = this;

        LoadChats();
    }

    private async void LoadChats()
    {
        var chats = await _chatService.GetUserChats(_authService.CurrentUser.Id);
        Chats.Clear();

        foreach (var chat in chats)
        {
            Chats.Add(chat);
        }
    }

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

    // Остальные команды остаются без изменений
    public ICommand SearchCommand => new Command<string>(async (query) =>
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            SearchResults.Clear();
            return;
        }

        var results = await _chatService.SearchUsers(query);
        SearchResults.Clear();

        foreach (var user in results)
        {
            SearchResults.Add(user);
        }
    });

    public ICommand OpenChatCommand => new Command<User>(async (user) =>
    {
        var chat = await _chatService.GetOrCreateDirectChat(user.Id);
        await Shell.Current.GoToAsync($"MessagePage?chatId={chat.Id}");
    });
}