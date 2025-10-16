using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hospital.Frontend.Pages.Account;

public class SignUpModel : PageModel
{
    private readonly IAuthApiClient _auth;

    [BindProperty]
    public SignupRequest Signup { get; set; } = new();

    public SignUpModel(IAuthApiClient auth)
    {
        _auth = auth;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostSignup(string? AdminPermissionsCsv)
    {
        if (!ModelState.IsValid) return Page();

        if (!string.IsNullOrWhiteSpace(AdminPermissionsCsv))
            Signup.Permissions = AdminPermissionsCsv
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

        var res = await _auth.SignupAsync(Signup);
        if (res.IsSuccessStatusCode)
        {
            TempData["Msg"] = "Account created. Please sign in.";
            return RedirectToPage("/Account/SignIn");
        }

        ModelState.AddModelError(string.Empty, "Sign-up failed. Check inputs or try again.");
        return Page();
    }
}
