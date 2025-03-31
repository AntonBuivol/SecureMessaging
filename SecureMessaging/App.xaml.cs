using Microsoft.Extensions.Logging;
using SecureMessaging.Services;
using SecureMessaging.Views;

namespace SecureMessaging;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<App> _logger;

    public App(IServiceProvider serviceProvider, ILogger<App> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        InitializeComponent();
        InitializeAppTheme();
        InitializeMainPage();

        // Отложенная инициализация после полной загрузки
        this.Dispatcher.Dispatch(async () =>
        {
            await InitializeAsync();
        });
    }

    private void InitializeAppTheme()
    {
        try
        {
            var settingsService = _serviceProvider.GetService<SettingsService>();
            var theme = settingsService?.GetTheme() ?? "Light";
            UserAppTheme = theme == "Dark" ? AppTheme.Dark : AppTheme.Light;
            _logger.LogInformation($"App theme initialized: {theme}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize app theme");
            UserAppTheme = AppTheme.Light;
        }
    }

    private void InitializeMainPage()
    {
        try
        {
            MainPage = _serviceProvider.GetService<AppShell>();
            _logger.LogInformation("Main page initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize main page");
            MainPage = new ContentPage
            {
                Content = new Label
                {
                    Text = "Application initialization error",
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center
                }
            };
        }
    }

    private async Task InitializeAsync()
    {
        try
        {
            // 1. Инициализация Supabase
            var supabase = _serviceProvider.GetService<SupabaseService>();
            if (supabase != null)
            {
                await supabase.InitializeAsync();
                _logger.LogInformation("Supabase initialized");
            }

            // 2. Инициализация чатов для существующих пользователей
            var chatService = _serviceProvider.GetService<ChatService>();
            if (chatService != null)
            {
                await chatService.InitializeExistingUsersChats(); // ← Добавьте эту строку
            }

            // 3. Проверка авторизации
            var authService = _serviceProvider.GetService<AuthService>();
            if (authService != null)
            {
                var isLoggedIn = await authService.IsLoggedIn();
                _logger.LogInformation($"Auth state: {isLoggedIn}");

                if (isLoggedIn)
                {
                    await Shell.Current.GoToAsync("///ChatListPage");
                    _logger.LogInformation("Navigated to ChatListPage");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "App initialization failed");
        }
    }

    protected override void OnStart()
    {
        base.OnStart();
        _logger.LogInformation("App started");

        // Дополнительная проверка при старте
        this.Dispatcher.Dispatch(async () =>
        {
            try
            {
                var authService = _serviceProvider.GetService<AuthService>();
                if (authService != null && await authService.IsLoggedIn())
                {
                    await Shell.Current.GoToAsync("///ChatListPage");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Startup auth check failed");
            }
        });
    }

    protected override void OnSleep()
    {
        base.OnSleep();
        _logger.LogInformation("App sleeping");
    }

    protected override void OnResume()
    {
        base.OnResume();
        _logger.LogInformation("App resuming");

        // Проверка авторизации при возобновлении
        this.Dispatcher.Dispatch(async () =>
        {
            try
            {
                var authService = _serviceProvider.GetService<AuthService>();
                if (authService != null)
                {
                    if (!await authService.IsLoggedIn())
                    {
                        await Shell.Current.GoToAsync("//MainPage");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Resume auth check failed");
            }
        });
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);

        window.Created += (s, e) => _logger.LogInformation("Window created");
        window.Activated += (s, e) => _logger.LogInformation("Window activated");
        window.Deactivated += (s, e) => _logger.LogInformation("Window deactivated");
        window.Stopped += (s, e) => _logger.LogInformation("Window stopped");
        window.Destroying += (s, e) => _logger.LogInformation("Window destroying");

        return window;
    }
}