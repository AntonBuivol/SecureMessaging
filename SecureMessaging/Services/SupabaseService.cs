using Supabase;
using Supabase.Postgrest;
using SecureMessaging.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Supabase.Interfaces;
using static Supabase.Postgrest.Constants;

namespace SecureMessaging.Services;

public class SupabaseService
{
    private readonly Supabase.Client _client;

    public SupabaseService()
    {
        var url = "https://hgmogmeywxfdrggfdfzl.supabase.co";
        var key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhnbW9nbWV5d3hmZHJnZ2ZkZnpsIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDMyNjMyNDMsImV4cCI6MjA1ODgzOTI0M30.75mo-uchFP1Mf9RzC-2Jn-De73Rn-agxpcofhSp2DWo";

        var options = new SupabaseOptions
        {
            AutoConnectRealtime = true
        };

        _client = new Supabase.Client(url, key, options);
    }

    public Supabase.Client Client => _client;

    public async Task InitializeAsync()
    {
        await _client.InitializeAsync();
    }
}