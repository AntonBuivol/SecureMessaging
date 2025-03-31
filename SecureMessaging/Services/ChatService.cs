using SecureMessaging.Models;
using SecureMessaging.Services;
using Supabase.Postgrest;

namespace SecureMessaging.Services;

public class ChatService
{
    private readonly SupabaseService _supabase;
    private readonly SignalRService _signalRService;

    public ChatService(SupabaseService supabase, SignalRService signalRService)
    {
        _supabase = supabase;
        _signalRService = signalRService;
    }


    private bool _chatsInitialized = false;

    public async Task InitializeExistingUsersChats()
    {
        if (_chatsInitialized) return;

        var allUsers = await _supabase.Client.From<User>().Get();
        var users = allUsers.Models;

        foreach (var user1 in users)
        {
            foreach (var user2 in users.Where(u => u.Id != user1.Id))
            {
                await GetOrCreateDirectChat(user1.Id, user2.Id);
            }
        }

        _chatsInitialized = true;
    }



    public async Task<List<Chat>> GetUserChatsWithParticipants(string userId)
    {
        // Получаем все чаты пользователя с участниками
        var response = await _supabase.Client
            .From<Chat>()
            .Filter("chat_participants.user_id", Supabase.Postgrest.Constants.Operator.Equals, userId)
            .Get();

        var chats = response.Models;

        // Для каждого чата загружаем участников
        foreach (var chat in chats)
        {
            var participantsResponse = await _supabase.Client
                .From<ChatParticipants>()
                .Filter("chat_id", Supabase.Postgrest.Constants.Operator.Equals, chat.Id)
                .Get();

            chat.ChatParticipants = participantsResponse.Models;

            // Загружаем данные пользователей
            var members = new List<User>();
            foreach (var participant in chat.ChatParticipants)
            {
                var user = await _supabase.Client
                    .From<User>()
                    .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, participant.UserId)
                    .Single();

                if (user != null)
                {
                    members.Add(user);
                }
            }
            chat.Members = members;
        }

        return chats;
    }

    public async Task<Chat> GetOrCreateDirectChat(string currentUserId, string targetUserId)
    {
        // 1. Получаем все чаты текущего пользователя
        var currentUserChats = await _supabase.Client
            .From<ChatParticipants>()
            .Filter(x => x.UserId, Constants.Operator.Equals, currentUserId)
            .Get();

        // 2. Проверяем каждый чат на наличие целевого пользователя
        foreach (var participant in currentUserChats.Models)
        {
            try
            {
                var targetParticipant = await _supabase.Client
                    .From<ChatParticipants>()
                    .Filter(x => x.ChatId, Constants.Operator.Equals, participant.ChatId)
                    .Filter(x => x.UserId, Constants.Operator.Equals, targetUserId)
                    .Single();

                if (targetParticipant != null)
                {
                    // Чат уже существует - возвращаем его
                    var existingChat = await _supabase.Client
                        .From<Chat>()
                        .Filter(x => x.Id, Constants.Operator.Equals, participant.ChatId)
                        .Single();

                    return existingChat;
                }
            }
            catch
            {
                // Участник не найден - продолжаем поиск
                continue;
            }
        }

        // 3. Чат не существует - создаем новый
        var newChat = new Chat
        {
            IsDirectMessage = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Получаем имя целевого пользователя для названия чата
        var targetUser = await _supabase.Client
            .From<User>()
            .Filter(x => x.Id, Constants.Operator.Equals, targetUserId)
            .Single();

        newChat.ChatName = targetUser?.DisplayName ?? targetUser?.Username;

        var chatResponse = await _supabase.Client.From<Chat>().Insert(newChat);
        var createdChat = chatResponse.Model;

        // 4. Добавляем участников
        await _supabase.Client.From<ChatParticipants>()
            .Insert(new ChatParticipants { ChatId = createdChat.Id, UserId = currentUserId });

        await _supabase.Client.From<ChatParticipants>()
            .Insert(new ChatParticipants { ChatId = createdChat.Id, UserId = targetUserId });

        return createdChat;
    }

    public async Task<List<Chat>> GetUserChats(string userId)
    {
        // First get all chat IDs where the user is a participant
        var participantResponse = await _supabase.Client
            .From<ChatParticipants>()
            .Filter(x => x.UserId, Constants.Operator.Equals, userId)
            .Get();

        var chatIds = participantResponse.Models.Select(p => p.ChatId).ToList();

        if (!chatIds.Any())
            return new List<Chat>();

        // Then get all chats with those IDs
        var chatResponse = await _supabase.Client
            .From<Chat>()
            .Filter(x => x.Id, Constants.Operator.In, chatIds)
            .Get();

        return chatResponse.Models;
    }

    public async Task<(Chat Chat, List<User> Participants)> CreateChat(string userId, List<string> participantIds)
    {
        var allParticipants = new List<string> { userId };
        allParticipants.AddRange(participantIds);

        // Create the chat
        var chat = new Chat
        {
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var response = await _supabase.Client
            .From<Chat>()
            .Insert(chat);

        var createdChat = response.Model;

        // Add participants
        foreach (var participantId in allParticipants)
        {
            await _supabase.Client
                .From<ChatParticipants>()
                .Insert(new ChatParticipants
                {
                    ChatId = createdChat.Id,
                    UserId = participantId
                });
        }

        // Get participants with user data
        var participants = await GetChatMembers(createdChat.Id);

        return (createdChat, participants);
    }

    public async Task<List<User>> GetChatMembers(string chatId)
    {
        var response = await _supabase.Client
            .From<ChatParticipants>()
            .Filter(x => x.ChatId, Constants.Operator.Equals, chatId)
            .Get();

        var members = new List<User>();
        foreach (var participant in response.Models)
        {
            var user = await _supabase.Client
                .From<User>()
                .Filter(x => x.Id, Constants.Operator.Equals, participant.UserId)
                .Single();

            if (user != null)
            {
                members.Add(user);
            }
        }

        return members;
    }

    public async Task<List<Message>> GetChatMessages(string chatId)
    {
        var response = await _supabase.Client
            .From<Message>()
            .Filter(x => x.ChatId, Constants.Operator.Equals, chatId)
            .Order(x => x.CreatedAt, Constants.Ordering.Ascending)
            .Get();

        return response.Models;
    }

    public async Task<Message> SendTextMessage(string chatId, string senderId, string text)
    {
        var message = new Message
        {
            ChatId = chatId,
            SenderId = senderId,
            Content = text,
            CreatedAt = DateTime.UtcNow
        };

        var response = await _supabase.Client
            .From<Message>()
            .Insert(message);

        var sentMessage = response.Model;
        await _signalRService.SendMessage(sentMessage);
        return sentMessage;
    }

    public async Task<Message> SendMediaMessage(string chatId, string senderId, string mediaUrl, string mediaType)
    {
        var message = new Message
        {
            ChatId = chatId,
            SenderId = senderId,
            MediaUrl = mediaUrl,
            MediaType = mediaType,
            CreatedAt = DateTime.UtcNow
        };

        var response = await _supabase.Client
            .From<Message>()
            .Insert(message);

        var sentMessage = response.Model;
        await _signalRService.SendMessage(sentMessage);
        return sentMessage;
    }
}