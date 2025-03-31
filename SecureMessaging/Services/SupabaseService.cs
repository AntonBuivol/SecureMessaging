using Supabase;
using Supabase.Postgrest;
using SecureMessaging.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Supabase.Interfaces;
using static Supabase.Postgrest.Constants;

namespace SecureMessaging.Services;

public class SupabaseService
{
    private readonly Supabase.Client _supabase;
    private readonly AuthService _authService;
    public Supabase.Client Client { get; }
    public SupabaseService()
    {
        var url = "https://hgmogmeywxfdrggfdfzl.supabase.co";
        var key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhnbW9nbWV5d3hmZHJnZ2ZkZnpsIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDMyNjMyNDMsImV4cCI6MjA1ODgzOTI0M30.75mo-uchFP1Mf9RzC-2Jn-De73Rn-agxpcofhSp2DWo";

        var options = new Supabase.SupabaseOptions
        {
            AutoConnectRealtime = true
        };

        _supabase = new Supabase.Client(url, key, options);
        Client = new Supabase.Client(url, key, options);
    }

    public async Task InitializeAsync()
    {
        await _supabase.InitializeAsync();

        // Явно указываем типы для кортежа
        (string userId, string deviceId) = await _authService.GetUserSession();

        if (!string.IsNullOrEmpty(userId))
        {
            await _supabase.From<User>()
                          .Where(x => x.Id == userId)
                          .Single();
        }
    }

    public async Task<List<User>> GetAllUsersExceptCurrent(string currentUserId)
    {
        var response = await _supabase
            .From<User>()
            .Filter(x => x.Id, Operator.NotEqual, currentUserId)
            .Get();

        return response.Models; // Извлекаем список пользователей из ответа
    }

    // User methods
    public async Task<User> GetUserById(string userId)
    {
        var response = await _supabase
            .From<User>()
            .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, userId)
            .Single();

        return response;
    }

    public async Task<User> GetUserByUsername(string username)
    {
        var response = await _supabase
            .From<User>()
            .Filter("username", Supabase.Postgrest.Constants.Operator.Equals, username)
            .Single();

        return response;
    }

    public async Task<bool> CheckUsernameExists(string username)
    {
        try
        {
            var response = await _supabase
                .From<User>()
                .Filter("username", Supabase.Postgrest.Constants.Operator.Equals, username)
                .Single();

            return response != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task CreateUser(User user, string passwordHash)
    {
        var newUser = new User
        {
            Username = user.Username,
            PasswordHash = passwordHash,
            DisplayName = user.DisplayName
        };

        await _supabase.From<User>().Insert(newUser);
    }

    public async Task UpdateUser(User user)
    {
        await _supabase.From<User>().Update(user);
    }

    // Device methods
    public async Task<UserDeviceInfo> GetCurrentDevice(string userId)
    {
        var response = await _supabase
            .From<UserDeviceInfo>()
            .Filter(x => x.UserId, Constants.Operator.Equals, userId)
            .Filter(x => x.IsCurrent, Constants.Operator.Equals, true)
            .Single();

        return response;
    }


    public async Task<List<UserDeviceInfo>> GetUserDevices(string userId)
    {
        var response = await _supabase
            .From<UserDeviceInfo>()
            .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, userId)
            .Get();

        return response.Models;
    }

    public async Task AddDevice(UserDeviceInfo device)
    {
        await _supabase.From<UserDeviceInfo>().Insert(device);
    }

    public async Task RemoveDevice(string deviceId)
    {
        await _supabase
            .From<UserDeviceInfo>()
            .Filter(x => x.Id, Constants.Operator.Equals, deviceId)
            .Delete();
    }


    public async Task SetCurrentDevice(string userId, string deviceId)
    {
        // Reset all devices for this user to not current
        await _supabase.From<UserDeviceInfo>()
            .Filter(x => x.UserId, Supabase.Postgrest.Constants.Operator.Equals, userId)
            .Set(x => x.IsCurrent, false)
            .Update();

        // Set the specified device as current
        await _supabase.From<UserDeviceInfo>()
            .Filter(x => x.Id, Supabase.Postgrest.Constants.Operator.Equals, deviceId)
            .Set(x => x.IsCurrent, true)
            .Set(x => x.LastLogin, DateTime.UtcNow)
            .Update();
    }

    // Chat methods
    public async Task<List<Chat>> GetUserChats(string userId)
    {
        var response = await _supabase
            .From<Chat>()
            .Filter("participants.user_id", Supabase.Postgrest.Constants.Operator.Equals, userId)
            .Get();

        return response.Models;
    }

    public async Task<Chat> CreateChat(List<string> participantIds)
    {
        var chat = new Chat
        {
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var response = await _supabase.From<Chat>().Insert(chat);

        // Add participants
        foreach (var participantId in participantIds)
        {
            await _supabase.From<ChatParticipants>()
                .Insert(new ChatParticipants
                {
                    ChatId = chat.Id,
                    UserId = participantId
                });
        }

        return response.Model;
    }

    public async Task<List<Message>> GetChatMessages(string chatId)
    {
        var response = await _supabase
            .From<Message>()
            .Filter("chat_id", Supabase.Postgrest.Constants.Operator.Equals, chatId)
            .Order("created_at", Supabase.Postgrest.Constants.Ordering.Ascending)
            .Get();

        return response.Models;
    }

    public async Task<Message> SendMessage(Message message)
    {
        var response = await _supabase
            .From<Message>()
            .Insert(message);

        return response.Model;
    }
}