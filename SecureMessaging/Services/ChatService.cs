using SecureMessaging.Models;
using SecureMessaging.Services;
using Supabase.Postgrest;
using static Supabase.Postgrest.Constants;

namespace SecureMessaging.Services;

public class ChatService
{
    private readonly SupabaseService _supabase;
    private readonly AuthService _authService;

    public ChatService(SupabaseService supabase, AuthService authService)
    {
        _supabase = supabase;
        _authService = authService;
    }

    public async Task<List<Chat>> GetUserChats(string userId)
    {
        // Получаем только ID чатов пользователя
        var participants = await _supabase.Client.From<ChatParticipant>()
            .Select("chat_id")
            .Filter("user_id", Operator.Equals, userId)
            .Get();

        if (!participants.Models.Any())
            return new List<Chat>();

        // Получаем полные данные чатов
        var chatIds = participants.Models.Select(p => p.ChatId).ToList();
        var chatsResponse = await _supabase.Client.From<Chat>()
            .Select("*, chat_participants(*)")
            .Filter("id", Operator.In, chatIds)
            .Get();

        return chatsResponse.Models;
    }

    public async Task<Chat> CreateGroupChat(List<string> memberIds, string chatName)
    {
        // Создаем чат
        var chat = new Chat
        {
            IsDirectMessage = false,
            ChatName = chatName
        };

        var chatResponse = await _supabase.Client.From<Chat>().Insert(chat);

        // Добавляем участников
        var participants = memberIds.Select(userId => new ChatParticipant
        {
            ChatId = chatResponse.Model.Id,
            UserId = userId
        }).ToList();

        await _supabase.Client.From<ChatParticipant>().Insert(participants);

        return chatResponse.Model;
    }


    public async Task<List<User>> SearchUsers(string query)
    {
        var response = await _supabase.Client.From<User>()
            .Filter("username", Operator.ILike, $"%{query}%")
            .Get();

        return response.Models.Where(u => u.Id != _authService.CurrentUser.Id).ToList();
    }

    public async Task<Chat> GetOrCreateDirectChat(string targetUserId)
    {
        try
        {
            var currentUserId = _authService.CurrentUser.Id;

            // Проверяем существующий чат
            var existingChat = await FindExistingDirectChat(currentUserId, targetUserId);
            if (existingChat != null)
                return existingChat;

            // Создаем новый чат
            return await CreateNewDirectChat(currentUserId, targetUserId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetOrCreateDirectChat: {ex.Message}");
            throw; // Перебрасываем исключение для обработки на уровне UI
        }
    }


    private async Task<Chat> FindExistingDirectChat(string userId1, string userId2)
    {
        try
        {
            // Получаем все чаты первого пользователя
            var user1ChatsResponse = await _supabase.Client.From<ChatParticipant>()
                .Select("chat!inner(*)")
                .Filter("user_id", Operator.Equals, userId1)
                .Filter("chat.is_direct_message", Operator.Equals, true)
                .Get();

            if (user1ChatsResponse.ResponseMessage.IsSuccessStatusCode)
            {
                // Проверяем каждый чат на наличие второго пользователя
                foreach (var participant in user1ChatsResponse.Models)
                {
                    var existsResponse = await _supabase.Client.From<ChatParticipant>()
                        .Filter("chat_id", Operator.Equals, participant.ChatId)
                        .Filter("user_id", Operator.Equals, userId2)
                        .Single();

                    if (existsResponse != null)
                    {
                        var chatResponse = await _supabase.Client.From<Chat>()
                            .Filter("id", Operator.Equals, participant.ChatId)
                            .Single();

                        return chatResponse;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding existing chat: {ex.Message}");
        }

        return null;
    }


    private async Task<Chat> CreateNewDirectChat(string userId1, string userId2)
    {
        try
        {
            var targetUser = await _supabase.Client.From<User>()
                .Filter("id", Operator.Equals, userId2)
                .Single();

            var newChat = new Chat
            {
                IsDirectMessage = true,
                ChatName = targetUser.DisplayName ?? targetUser.Username,
                CreatedAt = DateTime.UtcNow
            };

            var chatResponse = await _supabase.Client.From<Chat>().Insert(newChat);

            // Добавляем участников
            var participants = new List<ChatParticipant>
        {
            new ChatParticipant { ChatId = chatResponse.Model.Id, UserId = userId1, CreatedAt = DateTime.UtcNow },
            new ChatParticipant { ChatId = chatResponse.Model.Id, UserId = userId2, CreatedAt = DateTime.UtcNow }
        };

            await _supabase.Client.From<ChatParticipant>().Insert(participants);

            return chatResponse.Model;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating new chat: {ex.Message}");
            throw;
        }
    }

    public async Task<List<Message>> GetChatMessages(string chatId)
    {
        var response = await _supabase.Client.From<Message>()
            .Where(x => x.ChatId == chatId)
            .Order(x => x.CreatedAt, Ordering.Ascending)
            .Get();

        return response.Models;
    }

    public async Task<Message> SendMessage(string chatId, string text)
{
    var message = new Message
    {
        ChatId = chatId,
        SenderId = _authService.CurrentUser.Id,
        Content = text,
        CreatedAt = DateTime.UtcNow
    };

    var response = await _supabase.Client.From<Message>().Insert(message);
    return response.Model; // Извлекаем модель из ответа
}
}