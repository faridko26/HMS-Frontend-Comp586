// Services/AuthApiClient.cs
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

public class AuthApiClient : IAuthApiClient
{
    private readonly HttpClient _http;
    public AuthApiClient(HttpClient http) => _http = http;

    public Task<HttpResponseMessage> LoginAsync(LoginRequest req)
        => _http.PostAsJsonAsync("api/auth/login", req);

   }


//If backend uses different routes (e.g., /api/users/register), just update the two paths above.