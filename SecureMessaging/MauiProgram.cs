using Microsoft.Extensions.Logging;
using SecureMessaging.Services;
using SecureMessaging.Views;
using SecureMessaging.Views.Auth;

namespace SecureMessaging;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Регистрация сервисов
        builder.Services.AddSingleton<SupabaseService>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<ChatService>();
        builder.Services.AddSingleton<UserService>();

        // Регистрация страниц
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<LoginPage>();
        builder.Services.AddSingleton<RegisterPage>();
        builder.Services.AddSingleton<ChatListPage>();
        builder.Services.AddSingleton<ChatPage>();
        builder.Services.AddSingleton<ProfileSettingsPage>();
        builder.Services.AddSingleton<AppSettingsPage>();

        var app = builder.Build();

        // Инициализация Supabase
        var supabaseService = app.Services.GetRequiredService<SupabaseService>();
        supabaseService.InitializeAsync().ConfigureAwait(false);

        return app;
    }
}