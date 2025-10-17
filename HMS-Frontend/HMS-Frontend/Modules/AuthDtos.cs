using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

public class LoginRequest
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, MinLength(6)] public string Password { get; set; } = "";
}
