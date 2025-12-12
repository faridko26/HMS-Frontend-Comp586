using HMS_Frontend.Models;
using System.Net.Http.Json;

namespace HMS_Frontend.Services.Domain
{
    public interface IAppointmentService
    {
        Task<List<AppointmentDto>> GetAppointmentsAsync(DateTime? date = null);
        Task<(bool Success, string ErrorMessage)> CreateAppointmentAsync(CreateAppointmentDto appointment);
        Task<(bool Success, string ErrorMessage)> CancelAppointmentAsync(int id);
        Task<(bool Success, string ErrorMessage)> UpdateAppointmentAsync(int id, UpdateAppointmentDto appointment);
    }

    public class AppointmentService : IAppointmentService
    {
        private readonly HttpClient _http;
        private readonly ILogger<AppointmentService> _logger;

        public AppointmentService(HttpClient http, ILogger<AppointmentService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<AppointmentDto>> GetAppointmentsAsync(DateTime? date = null)
        {
            try
            {
                var url = "api/Appointments";
                if (date.HasValue)
                {
                    url += $"?date={date.Value:yyyy-MM-dd}";
                }

                var result = await _http.GetFromJsonAsync<List<AppointmentDto>>(url);
                return result ?? new List<AppointmentDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching appointments");
                return new List<AppointmentDto>();
            }
        }

        public async Task<(bool Success, string ErrorMessage)> CreateAppointmentAsync(CreateAppointmentDto appointment)
        {
            try
            {
                if (string.IsNullOrEmpty(appointment.Status)) appointment.Status = "Scheduled";

                var response = await _http.PostAsJsonAsync("api/Appointments", appointment);
                if (response.IsSuccessStatusCode) return (true, string.Empty);
                var error = await response.Content.ReadAsStringAsync();
                return (false, $"Failed to create appointment: {response.ReasonPhrase} - {error}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment");
                return (false, $"Internal error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> CancelAppointmentAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/Appointments/{id}");
                if (response.IsSuccessStatusCode) return (true, string.Empty);
                var error = await response.Content.ReadAsStringAsync();
                return (false, $"Failed to cancel appointment: {response.ReasonPhrase} - {error}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cancelling appointment {id}");
                return (false, $"Internal error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateAppointmentAsync(int id, UpdateAppointmentDto appointment)
        {
            try
            {
                var response = await _http.PutAsJsonAsync($"api/Appointments/{id}", appointment);
                if (response.IsSuccessStatusCode) return (true, string.Empty);
                var error = await response.Content.ReadAsStringAsync();
                return (false, $"Failed to update appointment: {response.ReasonPhrase} - {error}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating appointment {id}");
                return (false, $"Internal error: {ex.Message}");
            }
        }
    }
}
