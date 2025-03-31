using SecureMessaging.Models;
using SecureMessaging.Services;
using System.Diagnostics;

namespace SecureMessaging.Views;

public partial class MainPage : ContentPage
{
    private readonly AuthService _authService;
    private readonly SupabaseService _supabase;
    private readonly ChatService _chatService;

    public MainPage(AuthService authService, SupabaseService supabase, ChatService chatService)
    {
        InitializeComponent();
        _authService = authService;
        _supabase = supabase;
        _chatService = chatService;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Проверяем, нужно ли инициализировать чаты
        var needsInitialization = await CheckIfChatsInitializationNeeded();
        if (needsInitialization)
        {
            await _chatService.InitializeExistingUsersChats();
        }
        await CheckAuthState();
    }

    private async Task<bool> CheckIfChatsInitializationNeeded()
    {
        // Проверяем, есть ли пользователи без чатов
        var users = await _supabase.Client.From<User>().Get();
        var chats = await _supabase.Client.From<ChatParticipants>().Get();

        return users.Models.Any() && !chats.Models.Any();
    }


    public bool IsLoggedIn { get; private set; }

    private async Task CheckAuthState()
    {
        try
        {
            IsLoggedIn = await _authService.IsLoggedIn();
            OnPropertyChanged(nameof(IsLoggedIn));

            if (IsLoggedIn)
            {
                var (userId, deviceId) = await _authService.GetUserSession();

                if (!string.IsNullOrEmpty(deviceId))
                {
                    var currentDevice = await _supabase.Client
                        .From<UserDeviceInfo>()
                        .Where(x => x.Id == deviceId && x.IsCurrent)
                        .Single();

                    if (currentDevice != null)
                    {
                        await Shell.Current.GoToAsync("///ChatListPage");
                        return;
                    }
                }

                // Если дошли сюда - сессия невалидна
                await _authService.Logout();
                IsLoggedIn = false;
                OnPropertyChanged(nameof(IsLoggedIn));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Auth check error: {ex}");
            IsLoggedIn = false;
            OnPropertyChanged(nameof(IsLoggedIn));
        }
    }

    public Command GoToLoginCommand => new Command(async () =>
    {
        await Shell.Current.GoToAsync("///LoginPage");
    });

    public Command GoToRegisterCommand => new Command(async () =>
    {
        await Shell.Current.GoToAsync("///RegisterPage");
    });
}