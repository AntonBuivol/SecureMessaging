using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Collections.Generic;

namespace SecureMessaging.Models;

[Table("chats")]
public class Chat : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [Column("is_direct_message")]
    public bool IsDirectMessage { get; set; }

    [Column("chat_name")]
    public string ChatName { get; set; }

    // Связи участников чата (через промежуточную таблицу)
    [Reference(typeof(ChatParticipants))]
    public List<ChatParticipants> ChatParticipants { get; set; } = new();

    // Непосредственно пользователи-участники чата
    [Reference(typeof(User))]
    public List<User> Members { get; set; } = new();
}