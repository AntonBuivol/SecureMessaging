using Microsoft.Extensions.Logging;
using SecureMessaging.Services;
using SecureMessaging.Views;
using SecureMessaging.Views.Auth;
using Supabase;

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

        // Регистрируем сервисы
        builder.Services.AddSingleton<SettingsService>();
        builder.Services.AddSingleton<SupabaseService>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<ChatService>();
        builder.Services.AddSingleton<SearchService>();
        builder.Services.AddSingleton<SignalRService>();
        builder.Services.AddSingleton<IServiceProvider>(provider => provider);
        builder.Services.AddSingleton<UserService>();

        // Регистрируем страницы
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<LoginPage>();
        builder.Services.AddSingleton<RegisterPage>();
        builder.Services.AddSingleton<ChatListPage>();
        builder.Services.AddSingleton<ChatPage>();
        builder.Services.AddSingleton<ProfileSettingsPage>();
        builder.Services.AddSingleton<AppSettingsPage>();
        builder.Services.AddSingleton<MessagePage>();

        builder.Logging.AddDebug(); // Для логгирования
        builder.Services.AddLogging(); // Добавляем сервис логгирования

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}