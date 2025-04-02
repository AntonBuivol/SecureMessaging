using SecureMessaging.Models;
using Supabase.Postgrest;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;

namespace SecureMessaging.Services;

public class UserService
{
    private readonly Supabase.Client _client;

    public UserService(SupabaseService supabaseService)
    {
        _client = supabaseService.Client;
    }

    public async Task<List<User>> GetAllUsersExceptCurrent(string currentUserId)
    {
        var response = await _client
            .From<User>()
            .Filter("id", Operator.NotEqual, currentUserId)
            .Order("username", Ordering.Ascending)
            .Get();

        return response.Models;
    }
}