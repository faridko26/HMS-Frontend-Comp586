using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HMS_Frontend.Models
{
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
        [Required(ErrorMessage = "Full Name is required")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Username is required")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public string? Role { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string? Password { get; set; }
    }

    public class UpdateUserRequest
    {
        [Required(ErrorMessage = "Full Name is required")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Username is required")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public string? Role { get; set; }

        public bool IsActive { get; set; }

        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string? Password { get; set; }
    }
}
