using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

public class LoginRequest
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, MinLength(6)] public string Password { get; set; } = "";
}

public class SignupRequest
{
    [Required] public string FirstName { get; set; } = "";
    [Required] public string LastName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, MinLength(6)] public string Password { get; set; } = "";
    [Required] public string Role { get; set; } = ""; // Doctor, Nurse, Receptionist, Admin
    [Phone] public string? PhoneNumber { get; set; }
    public string? Building { get; set; }

    // Doctor-only
    public string? Specialization { get; set; }
    public string? RoomNumber { get; set; }

    // Admin-only (simple string list for now)
    public List<string>? Permissions { get; set; }
}
