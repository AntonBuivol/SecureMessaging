using SecureMessaging.Models;
using Supabase.Postgrest;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecureMessaging.Services;

public class SearchService
{
    private readonly SupabaseService _supabase;
    private readonly ChatService _chatService;

    public SearchService(SupabaseService supabase, ChatService chatService)
    {
        _supabase = supabase;
        _chatService = chatService;
    }

    public async Task<List<User>> SearchUsers(string query)
    {
        var response = await _supabase.Client
            .From<User>()
            .Filter(x => x.Username, Constants.Operator.ILike, $"%{query}%")
            .Get();

        return response.Models;
    }

    public async Task<Chat> StartDirectMessage(string currentUserId, string targetUserId, string targetUsername, string targetDisplayName)
    {
        // Check if a direct message chat already exists
        var existingChatResponse = await _supabase.Client
            .From<ChatParticipants>()
            .Filter(x => x.UserId, Constants.Operator.Equals, currentUserId)
            .Get();

        var existingChats = existingChatResponse.Models;

        foreach (var chat in existingChats)
        {
            var participantsResponse = await _supabase.Client
                .From<ChatParticipants>()
                .Filter(x => x.ChatId, Constants.Operator.Equals, chat.ChatId)
                .Get();

            if (participantsResponse.Models.Any(p => p.UserId == targetUserId))
            {
                // Chat already exists, return it
                var existingChat = await _supabase.Client
                    .From<Chat>()
                    .Filter(x => x.Id, Constants.Operator.Equals, chat.ChatId)
                    .Single();

                return existingChat;
            }
        }

        // Create new direct message chat
        var newChat = new Chat
        {
            IsDirectMessage = true,
            ChatName = string.IsNullOrEmpty(targetDisplayName) ? targetUsername : targetDisplayName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var chatResponse = await _supabase.Client.From<Chat>().Insert(newChat);
        var createdChat = chatResponse.Model;

        // Add participants
        await _supabase.Client.From<ChatParticipants>()
            .Insert(new ChatParticipants { ChatId = createdChat.Id, UserId = currentUserId });

        await _supabase.Client.From<ChatParticipants>()
            .Insert(new ChatParticipants { ChatId = createdChat.Id, UserId = targetUserId });

        return createdChat;
    }
}