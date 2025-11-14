using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HMS_Frontend.Pages.Account;

public class SignInModel : PageModel
{
    private readonly IAuthApiClient _auth;

    [BindProperty]
    public LoginRequest Login { get; set; } = new();

    public SignInModel(IAuthApiClient auth)
    {
        _auth = auth;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostLogin()
    {
        if (!ModelState.IsValid) return Page();

        var token = await _auth.LoginAsync(Login);
        if (string.IsNullOrEmpty(token))
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return Page();
        }

        // Print the token to the console for debugging
        Console.WriteLine("JWT Token: " + token);


        // Decode JWT token
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var role = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;

        if (string.IsNullOrEmpty(role))
        {
            ModelState.AddModelError(string.Empty, "User role not found.");
            return Page();
        }

        // Optionally store token in session for later API calls
        HttpContext.Session.SetString("JwtToken", token);
        HttpContext.Session.SetString("UserRole", role);

        // Redirect based on role
        return role.ToLower() switch
        {
            "receptionist" => RedirectToPage("/Receptionist"),
            "doctor" => RedirectToPage("/Doctor/Index"),
            "admin" => RedirectToPage("/Admin"),
            _ => RedirectToPage("/Account/AccessDenied")
        };
    }
}