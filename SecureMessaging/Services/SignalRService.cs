using Microsoft.AspNetCore.SignalR.Client;
using SecureMessaging.Models;

namespace SecureMessaging.Services;

public class SignalRService
{
    private HubConnection _hubConnection;

    public async Task InitializeAsync(string userId)
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("YOUR_SIGNALR_HUB_URL")
            .Build();

        await _hubConnection.StartAsync();
    }

    public async Task DisconnectAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
        }
    }
}