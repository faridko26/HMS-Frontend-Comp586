// Services/UsersApiClient.cs
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

public class UsersApiClient : IUsersApiClient
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public UsersApiClient(HttpClient http) => _http = http;

    public async Task<List<UserDto>> GetAllAsync()
    {
        var res = await _http.GetAsync("api/users");
        if (!res.IsSuccessStatusCode) return new();
        return await res.Content.ReadFromJsonAsync<List<UserDto>>(_json) ?? new();
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var res = await _http.GetAsync($"api/users/{id}");
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<UserDto>(_json);
    }

    public async Task<bool> CreateAsync(CreateUserRequest req)
    {
        var res = await _http.PostAsJsonAsync("api/users", req, _json);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateUserRequest req)
    {
        var res = await _http.PutAsJsonAsync($"api/users/{id}", req, _json);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> DeactivateAsync(Guid id)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, $"api/users/{id}/deactivate")
        {
            Content = new StringContent("", Encoding.UTF8, "application/json")
        };
        var res = await _http.SendAsync(request);
        // TEMP debug
        Console.WriteLine($"PATCH deactivate {id} -> {(int)res.StatusCode}");
        if (!res.IsSuccessStatusCode)
        {
            var body = await res.Content.ReadAsStringAsync();
            Console.WriteLine("Error body: " + body);
        }
        return res.IsSuccessStatusCode;
    }
}
