// Models/UsersDtos.cs
using System.Text.Json.Serialization;

public record UserDto(
    [property: JsonPropertyName("userId")]
    int Id,
    [property: JsonPropertyName("fullName")]
    string FullName,
    [property: JsonPropertyName("email")]
    string Email,
    [property: JsonPropertyName("username")]
    string Username,
    [property: JsonPropertyName("role")]
    string Role,
    [property: JsonPropertyName("isActive")]
    bool IsActive
);

public class CreateUserRequest
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Username { get; set; }
    public string? Role { get; set; }
    public string? Password { get; set; }
}

public class UpdateUserRequest
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Username { get; set; }
    public string? Role { get; set; }

    // This property is from the error before
    public bool IsActive { get; set; }

    // THIS IS THE PROPERTY THE COMPILER SAYS IS MISSING
    public string? Password { get; set; }
}
