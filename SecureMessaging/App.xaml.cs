using Microsoft.Extensions.Logging;
using SecureMessaging.Services;
using SecureMessaging.Views;

namespace SecureMessaging;

public partial class App : Application
{
    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        MainPage = serviceProvider.GetService<AppShell>();
    }
}