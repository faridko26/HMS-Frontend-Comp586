// Services/UsersApiClient.cs
using HMS_Frontend.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

public class UsersApiClient : IUsersApiClient
{

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public UsersApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
    {
        _http = http;
        _httpContextAccessor = httpContextAccessor;
    }


    private void SetAuthorizationHeader()
    {
        // FROM:
        // var token = _httpContextAccessor.HttpContext?.Session.GetString("AccessToken");

        // TO: (Add a '?' after Session)
        var token = _httpContextAccessor.HttpContext?.Session?.GetString("AccessToken");

        if (!string.IsNullOrEmpty(token))
        {
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }
    public async Task<List<UserDto>> GetAllAsync()
    {
        //SetAuthorizationHeader();
        var res = await _http.GetAsync("api/Users");

        // This block will catch 401, 404, 500 errors
        if (!res.IsSuccessStatusCode)
        {
            Console.WriteLine($"[API Client] GetAllAsync FAILED. Status: {(int)res.StatusCode}");
            var errorBody = await res.Content.ReadAsStringAsync();
            Console.WriteLine($"[API Client] Error Body: {errorBody}");
            return new(); // Return empty list
        }

        // If we get here, the call was a 200 OK.
        // Let's log the successful JSON.
        string rawJson = await res.Content.ReadAsStringAsync();
        Console.WriteLine("-----------------------------------------------------");
        Console.WriteLine("[API Client] GetAllAsync SUCCESS. Raw JSON is:");
        Console.WriteLine(rawJson); // This is the JSON we need to see
        Console.WriteLine("-----------------------------------------------------");

        // Re-create content to allow deserialization
        res.Content = new StringContent(rawJson, System.Text.Encoding.UTF8, "application/json");

        return await res.Content.ReadFromJsonAsync<List<UserDto>>(_json) ?? new();
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var res = await _http.GetAsync($"api/Users/{id}");
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<UserDto>(_json);
    }

    public async Task<bool> CreateAsync(CreateUserRequest req)
    {
        var res = await _http.PostAsJsonAsync("api/Users", req, _json);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(int id, UpdateUserRequest req)
    {
        SetAuthorizationHeader();
        var res = await _http.PutAsJsonAsync($"api/Users/{id}", req, _json);

        if (!res.IsSuccessStatusCode)
        {
            Console.WriteLine($"[API update Client] UpdateAsync FAILED. Status: {(int)res.StatusCode}");
            var errorBody = await res.Content.ReadAsStringAsync();
            Console.WriteLine($"[API update Client] Error Body: {errorBody}");
        }

        return res.IsSuccessStatusCode;
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        SetAuthorizationHeader(); // <-- ADD THIS LINE
        var request = new HttpRequestMessage(HttpMethod.Patch, $"api/Users/{id}/deactivate")
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
