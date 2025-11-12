// Pages/Admin.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HMS_Frontend.Pages
{
    public class AdminModel : PageModel
    {
        private readonly IUsersApiClient _users;

        public AdminModel(IUsersApiClient users) => _users = users;

        public List<UserDto> Users { get; set; } = new();

        // Form bindings
        [BindProperty] public CreateUserInput CreateInput { get; set; } = new();
        [BindProperty] public UpdateUserInput UpdateInput { get; set; } = new();
        [BindProperty] public Guid Id { get; set; } // used by Update/Deactivate

        public async Task OnGet()
        {
            Users = await _users.GetAllAsync();
        }

        public async Task<IActionResult> OnPostCreate()
        {
            if (!ModelState.IsValid) return await Reload();

            var ok = await _users.CreateAsync(new CreateUserRequest
            {
                FullName = CreateInput.FullName,
                Email = CreateInput.Email,
                Username = CreateInput.Username,
                Role = CreateInput.Role,
                Password = CreateInput.Password
            });

            if (!ok)
            {
                ModelState.AddModelError(string.Empty, "Create failed.");
                return await Reload();
            }

            TempData["Msg"] = "User created.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdate()
        {
            if (!ModelState.IsValid || Id == Guid.Empty) return await Reload();

            var ok = await _users.UpdateAsync(Id, new UpdateUserRequest
            {
                FullName = UpdateInput.FullName,
                Email = UpdateInput.Email,
                Username = UpdateInput.Username,
                Role = UpdateInput.Role
            });

            if (!ok)
            {
                ModelState.AddModelError(string.Empty, "Update failed.");
                return await Reload();
            }

            TempData["Msg"] = "User updated.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeactivate()
        {
            if (Id == Guid.Empty) return await Reload();

            var ok = await _users.DeactivateAsync(Id);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, "Deactivate failed.");
                return await Reload();
            }

            TempData["Msg"] = "User deactivated.";
            return RedirectToPage();
        }

        public IActionResult OnPostSignOut()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Account/SignIn");
        }

        private async Task<IActionResult> Reload()
        {
            Users = await _users.GetAllAsync();
            return Page();
        }
    }

    // ------- Inputs bound to Razor forms -------
    public class CreateUserInput
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Username { get; set; } = "";
        public string Role { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class UpdateUserInput
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Username { get; set; } = "";
        public string Role { get; set; } = "";
    }
}
