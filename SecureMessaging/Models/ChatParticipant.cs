using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureMessaging.Models;

[Table("chat_participants")]
public class ChatParticipant : BaseModel
{
    [Column("chat_id")]
    public string ChatId { get; set; }

    [Column("user_id")]
    public string UserId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Reference(typeof(User))]
    public User user { get; set; }  // важно: lowercase

    [Reference(typeof(Chat))]
    public Chat chat { get; set; }   // важно: lowercase
}