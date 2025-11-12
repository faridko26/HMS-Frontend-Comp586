// Models/UsersDtos.cs
public record UserDto(
    Guid Id,
    string FullName,
    string Email,
    string Username,
    string Role,
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
}
