using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hospital.Frontend.Pages.Account;

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

        var res = await _auth.LoginAsync(Login);
        if (res.IsSuccessStatusCode)
        {
            TempData["Msg"] = "Logged in!";
            return RedirectToPage("/Dashboard"); // or wherever your homepage is
        }

        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return Page();
    }
}
