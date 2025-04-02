using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SecureMessaging.Models;

[Table("users")]
public class User : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; }

    [Column("username")]
    public string Username { get; set; }

    [Column("password_hash")]
    public string PasswordHash { get; set; }

    [Column("display_name")]
    public string DisplayName { get; set; }
}