using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SecureMessaging.Models;

[Table("messages")]
public class Message : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; }

    [Column("chat_id")]
    public string ChatId { get; set; }

    [Column("sender_id")]
    public string SenderId { get; set; }

    [Column("content")]
    public string Content { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
