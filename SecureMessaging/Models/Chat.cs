using AutoMapper.Configuration.Annotations;
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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("is_direct_message")]
    public bool IsDirectMessage { get; set; }

    [Column("chat_name")]
    public string ChatName { get; set; }

    // Важно: название свойства должно совпадать с именем таблицы в БД
    [Reference(typeof(ChatParticipant))]
    public List<ChatParticipant> chat_participants { get; set; } = new();
}