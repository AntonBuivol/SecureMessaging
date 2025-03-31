using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SecureMessaging.Models;

[Table("devices")]
public class UserDeviceInfo : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; }

    [Column("user_id")]
    public string UserId { get; set; }

    [Column("device_name")]
    public string DeviceName { get; set; }

    [Column("device_model")]
    public string DeviceModel { get; set; }

    [Column("os_version")]
    public string OsVersion { get; set; }

    [Column("device_id")]
    public string DeviceId { get; set; }

    [Column("is_current")]
    public bool IsCurrent { get; set; }

    [Column("last_login")]
    public DateTime LastLogin { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}