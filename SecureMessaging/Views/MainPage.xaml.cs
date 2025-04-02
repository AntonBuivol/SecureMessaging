using SecureMessaging.Models;
using SecureMessaging.Services;
using System.Diagnostics;

namespace SecureMessaging.Views;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    public Command GoToLoginCommand => new Command(async () =>
    {
        await Shell.Current.GoToAsync("//LoginPage");
    });

    public Command GoToRegisterCommand => new Command(async () =>
    {
        await Shell.Current.GoToAsync("//RegisterPage");
    });
}