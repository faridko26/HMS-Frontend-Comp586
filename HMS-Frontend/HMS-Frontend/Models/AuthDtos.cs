using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace HMS_Frontend.Models
{
    public class LoginRequest
    {
        [Required] public string Username { get; set; } = string.Empty;
        [Required, MinLength(6)] public string Password { get; set; } = string.Empty;
    }
}
