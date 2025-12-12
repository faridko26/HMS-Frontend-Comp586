using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using HMS_Frontend.Services;
using HMS_Frontend.Models;

namespace HMS_Frontend.Pages.Account;

public class SignInModel : PageModel
{
    private readonly IAuthApiClient _auth;
    private readonly IApiService _api;

    [BindProperty]
    public LoginRequest Login { get; set; } = new();

    public SignInModel(IAuthApiClient auth, IApiService api)
    {
        _auth = auth;
        _api = api;
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

        Console.WriteLine("--- JWT Claims ---");
        foreach (var claim in jwt.Claims)
        {
            Console.WriteLine($"{claim.Type}: {claim.Value}");
        }
        Console.WriteLine("------------------");

        var role = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;

        if (string.IsNullOrEmpty(role))
        {
            ModelState.AddModelError(string.Empty, "User role not found.");
            return Page();
        }

        // Optionally store token in session for later API calls
        HttpContext.Session.SetString("JwtToken", token);
        HttpContext.Session.SetString("UserRole", role);

        // Create ClaimsPrincipal for Cookie Auth
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, Login.Username),
            new Claim(ClaimTypes.Role, role),
            new Claim("JwtToken", token)
        };

        // Try to fetch full name
        try
        {
            var staff = await _api.GetStaffAsync();
            var user = staff.FirstOrDefault(u => u.Username.Equals(Login.Username, StringComparison.OrdinalIgnoreCase));
            if (user != null)
            {
                claims.Add(new Claim("FullName", user.FullName));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SignIn] Error fetching full name: {ex.Message}");
        }

        var claimsIdentity = new ClaimsIdentity(claims, Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
        };

        await HttpContext.SignInAsync(
            Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        // Redirect based on role
        var normalizedRole = role.Trim().ToLowerInvariant();
        Console.WriteLine($"[Login] Redirecting role '{role}' (normalized: '{normalizedRole}')");

        return normalizedRole switch
        {
            "receptionist" => RedirectToPage("/Dashboard"),
            "doctor" => RedirectToPage("/Dashboard"),
            "admin" => RedirectToPage("/Admin"),
            "nurse" => RedirectToPage("/Dashboard"),
            _ => RedirectToPage("/Account/AccessDenied")
        };
    }
}