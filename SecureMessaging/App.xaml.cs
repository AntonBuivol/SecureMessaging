using Microsoft.Extensions.Logging;
using SecureMessaging.Services;
using SecureMessaging.Views;

namespace SecureMessaging;

public partial class App : Application
{
    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();

        // Получаем сервисы после инициализации
        var supabaseService = serviceProvider.GetRequiredService<SupabaseService>();
        supabaseService.InitializeAsync().ConfigureAwait(false);

        MainPage = serviceProvider.GetService<AppShell>();
    }
}