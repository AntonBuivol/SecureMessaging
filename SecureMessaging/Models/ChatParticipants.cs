using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SecureMessaging.Models;

[Table("chat_participants")]
public class ChatParticipants : BaseModel
{
    [Column("chat_id")]
    public string ChatId { get; set; }

    [Column("user_id")]
    public string UserId { get; set; }

    [Reference(typeof(User))]
    public User User { get; set; }
}