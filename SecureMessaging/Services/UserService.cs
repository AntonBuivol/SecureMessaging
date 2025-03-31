using SecureMessaging.Models;
using Supabase.Postgrest;

namespace SecureMessaging.Services;

public class UserService
{
    private readonly Supabase.Client _client;

    public UserService(Supabase.Client client)
    {
        _client = client;
    }

    public async Task<List<User>> GetAllUsersExceptCurrent(string currentUserId)
    {
        var response = await _client
            .From<User>()
            .Filter("id", Supabase.Postgrest.Constants.Operator.NotEqual, currentUserId)
            .Get();

        return response.Models;
    }
}