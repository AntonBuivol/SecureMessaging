using Microsoft.AspNetCore.SignalR.Client;
using SecureMessaging.Models;

namespace SecureMessaging.Services;

public class SignalRService
{
    private HubConnection _hubConnection;

    public event Action<Message> MessageReceived;

    public async Task InitializeAsync(string userId)
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("YOUR_SIGNALR_HUB_URL")
            .Build();

        _hubConnection.On<Message>("ReceiveMessage", (message) =>
        {
            MessageReceived?.Invoke(message);
        });

        await _hubConnection.StartAsync();
        await _hubConnection.InvokeAsync("RegisterUser", userId);
    }

    public async Task SendMessage(Message message)
    {
        if (_hubConnection.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("SendMessage", message);
        }
    }

    public async Task DisconnectAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
        }
    }
}