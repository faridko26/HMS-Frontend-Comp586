// Services/AuthApiClient.cs
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

public class AuthApiClient : IAuthApiClient
{
    private readonly HttpClient _http;
    public AuthApiClient(HttpClient http) => _http = http;

    public async Task<string?> LoginAsync(LoginRequest req)
    {
        var res = await _http.PostAsJsonAsync("auth/login", req);

        if (!res.IsSuccessStatusCode)
            return null;

        // read token
        var content = await res.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        if (content != null && content.TryGetValue("token", out var token))
            return token;

        return null;
    }

}


//If backend uses different routes (e.g., /api/users/register), just update the two paths above.