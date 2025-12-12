using HMS_Frontend.Models;
using System.Net.Http.Json;

namespace HMS_Frontend.Services.Domain
{
    public interface IStaffService
    {
        Task<List<StaffUserDto>> GetDoctorsAsync();
        Task<List<StaffUserDto>> GetStaffAsync();
        Task<(bool Success, string ErrorMessage)> UpdateUserAsync(UserAccountDto user);
    }

    public class StaffService : IStaffService
    {
        private readonly HttpClient _http;
        private readonly ILogger<StaffService> _logger;

        public StaffService(HttpClient http, ILogger<StaffService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<StaffUserDto>> GetStaffAsync()
        {
            try
            {
                var allUsers = await _http.GetFromJsonAsync<List<StaffUserDto>>("api/Users");
                if (allUsers == null) return new List<StaffUserDto>();

                return allUsers.Where(u =>
                    !string.IsNullOrEmpty(u.Role) &&
                    (u.Role.Equals("Doctor", StringComparison.OrdinalIgnoreCase) ||
                     u.Role.Equals("Nurse", StringComparison.OrdinalIgnoreCase) ||
                     u.Role.Equals("Receptionist", StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching staff");
                return new List<StaffUserDto>();
            }
        }

        public async Task<List<StaffUserDto>> GetDoctorsAsync()
        {
            try
            {
                var allUsers = await _http.GetFromJsonAsync<List<StaffUserDto>>("api/Users");
                if (allUsers == null) return new List<StaffUserDto>();

                foreach (var u in allUsers)
                {
                    u.FullName = $"{u.FullName} [{u.Role}]";
                }
                return allUsers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching doctors");
                return new List<StaffUserDto>
                {
                    new StaffUserDto { UserId = -1, FullName = $"ERROR: {ex.Message}", Role = "Error" }
                };
            }
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateUserAsync(UserAccountDto user)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Users", user);
                if (response.IsSuccessStatusCode) return (true, string.Empty);
                var error = await response.Content.ReadAsStringAsync();
                return (false, $"Failed to update user: {response.ReasonPhrase} - {error}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                return (false, $"Internal error: {ex.Message}");
            }
        }
    }
}
