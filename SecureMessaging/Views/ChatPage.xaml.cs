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

        if (Shell.Current.CurrentState.Location.OriginalString.Contains("chatId="))
        {
            var query = Shell.Current.CurrentState.Location.Query;
            if (QueryStringHelper.GetQueryParam(query, "chatId") is string chatId)
            {
                _chatId = chatId;
                await LoadMessages();
            }
        }
    }

    public static class QueryStringHelper
    {
        public static string GetQueryParam(string query, string paramName)
        {
            if (string.IsNullOrWhiteSpace(query)) return null;

            var queryParams = System.Web.HttpUtility.ParseQueryString(query);
            return queryParams[paramName];
        }
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
            await _chatService.SendMessage(_chatId, MessageText);
            MessageText = string.Empty;
            OnPropertyChanged(nameof(MessageText));
            await LoadMessages();
        }
    });
}