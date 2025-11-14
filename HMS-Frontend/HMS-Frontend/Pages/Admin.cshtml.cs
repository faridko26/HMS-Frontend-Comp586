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

      
        [BindProperty] public CreateUserRequest CreateInput { get; set; } = new();
        [BindProperty] public UpdateUserRequest UpdateInput { get; set; } = new();
        [BindProperty] public int Id { get; set; } // used by Update/Deactivate

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
            // We already added this fix, but make sure it's here
            if (!ModelState.IsValid || Id == 0)
            {
                // --- ADD THIS LOGGING ---
                Console.WriteLine("[SERVER] OnPostUpdate was called but ModelState is INVALID.");
                foreach (var entry in ModelState)
                {
                    if (entry.Value.Errors.Count > 0)
                    {
                        Console.WriteLine($"[SERVER] Error for key '{entry.Key}':");
                        foreach (var error in entry.Value.Errors)
                        {
                            Console.WriteLine($"  - {error.ErrorMessage}");
                        }
                    }
                }
                // -------------------------

                UpdateInput = new UpdateUserRequest(); // <-- NEW
                return await Reload();
            }
            // ?? ADD THIS LINE to see what the server receives
            Console.WriteLine($"[SERVER] OnPostUpdate: Role from form is '{UpdateInput.Role}'");
            // --- Modify the UpdateAsync call ---

            // 1. Create the request object
            var updateRequest = new UpdateUserRequest
            {
                FullName = UpdateInput.FullName,
                Email = UpdateInput.Email,
                Username = UpdateInput.Username,
                Role = UpdateInput.Role,
                IsActive = UpdateInput.IsActive // Pass the status
            };

            // 2. Only add password if user entered a new one
            if (!string.IsNullOrEmpty(UpdateInput.Password))
            {
                updateRequest.Password = UpdateInput.Password;
            }

            // 3. Send the complete request
            var ok = await _users.UpdateAsync(Id, updateRequest);

            if (!ok)
            {
                ModelState.AddModelError(string.Empty, "Update failed.");
                UpdateInput = new UpdateUserRequest(); // <-- NEW
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
