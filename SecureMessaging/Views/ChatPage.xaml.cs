using SecureMessaging.Models;
using SecureMessaging.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SecureMessaging.Views;

public partial class ChatPage : ContentPage
{
    private readonly ChatService _chatService;
    private readonly AuthService _authService;
    private string _chatId;

    public ObservableCollection<Message> Messages { get; } = new();
    public string MessageText { get; set; }

    public ChatPage(ChatService chatService, AuthService authService)
    {
        InitializeComponent();
        _chatService = chatService;
        _authService = authService;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _chatId = Shell.Current.CurrentState.Location.OriginalString.Split('=')[1];
        await LoadMessages();
    }

    private async Task LoadMessages()
    {
        var messages = await _chatService.GetChatMessages(_chatId);
        Messages.Clear();

        foreach (var message in messages)
        {
            Messages.Add(message);
        }
    }

    public ICommand SendMessageCommand => new Command(async () =>
    {
        if (!string.IsNullOrWhiteSpace(MessageText))
        {
            await _chatService.SendMessage(_chatId, MessageText); // Убрали senderId
            MessageText = string.Empty;
            OnPropertyChanged(nameof(MessageText));
            await LoadMessages();
        }
    });
}