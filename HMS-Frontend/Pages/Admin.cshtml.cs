// Pages/Admin.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HMS_Frontend.Models;

namespace HMS_Frontend.Pages
{
    public class AdminModel : PageModel
    {
        private readonly IUsersApiClient _users;

        public AdminModel(IUsersApiClient users) => _users = users;

        public List<UserDto> Users { get; set; } = new();


        [BindProperty] public CreateUserRequest CreateInput { get; set; } = new();
        [BindProperty] public UpdateUserRequest UpdateInput { get; set; } = new();
        [BindProperty] public int Id { get; set; } // used by Update/Deactivate

        public async Task OnGet()
        {
            Users = await _users.GetAllAsync();
        }

        public async Task<IActionResult> OnPostCreate()
        {
            // 1. Clear all errors (including UpdateInput's)
            ModelState.Clear();

            // 2. Validate ONLY CreateInput
            if (!TryValidateModel(CreateInput, nameof(CreateInput)))
            {
                return await Reload();
            }

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

        // This handler is for JavaScript to call
        public async Task<IActionResult> OnGetUserDetails(int id)
        {
            // Assuming your client has a method to get a single user.
            // If not, you'll need to add one.
            // This call is the "GET /api/users/{id}" you wanted.
            var user = await _users.GetByIdAsync(id); // You may need to create this method

            if (user == null)
            {
                return NotFound();
            }

            // Return the user data as JSON
            return new JsonResult(user);
        }

        public async Task<IActionResult> OnPostUpdate()
        {
            Console.WriteLine($"[SERVER] OnPostUpdate Called. Id: {Id}");

            // 1. Clear all errors (including CreateInput's)
            ModelState.Clear();

            // 2. Validate ONLY UpdateInput
            if (!TryValidateModel(UpdateInput, nameof(UpdateInput)))
            {
                // If validation failed, check if it's just the empty password
                if (string.IsNullOrEmpty(UpdateInput.Password))
                {
                    // Remove the password error
                    ModelState.Remove("UpdateInput.Password");

                    // Re-check validity after removal
                    if (ModelState.IsValid)
                    {
                        // It was only the password, so proceed
                        goto ProceedUpdate;
                    }
                }

                // If we are here, there are other errors
                Console.WriteLine("[SERVER] OnPostUpdate: Validation failed.");
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        Console.WriteLine($"  - {entry.Key}: {error.ErrorMessage}");
                    }
                }
                UpdateInput = new UpdateUserRequest();
                return await Reload();
            }

        ProceedUpdate:
            if (Id == 0)
            {
                ModelState.AddModelError(string.Empty, "Invalid User ID.");
                return await Reload();
            }

            // 3. Send the complete request
            var updateRequest = new UpdateUserRequest
            {
                FullName = UpdateInput.FullName,
                Email = UpdateInput.Email,
                Username = UpdateInput.Username,
                Role = UpdateInput.Role,
                IsActive = UpdateInput.IsActive
            };

            if (!string.IsNullOrEmpty(UpdateInput.Password))
            {
                updateRequest.Password = UpdateInput.Password;
            }

            var ok = await _users.UpdateAsync(Id, updateRequest);

            if (!ok)
            {
                ModelState.AddModelError(string.Empty, "Update failed.");
                UpdateInput = new UpdateUserRequest();
                return await Reload();
            }

            TempData["Msg"] = "User updated.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeactivate()
        {
            if (Id == 0) return await Reload();

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

}
