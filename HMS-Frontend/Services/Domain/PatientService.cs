using HMS_Frontend.Models;
using System.Net.Http.Json;

namespace HMS_Frontend.Services.Domain
{
    public interface IPatientService
    {
        Task<List<PatientDto>> SearchPatientsAsync(string query);
        Task<List<PatientDto>> GetAllPatientsAsync();
        Task<PatientDto?> GetPatientAsync(int id);
        Task<(bool Success, string ErrorMessage)> CreatePatientAsync(CreatePatientDto patient);
        Task<(bool Success, string ErrorMessage)> UpdatePatientAsync(int id, UpdatePatientDto patient);
        Task<(bool Success, string ErrorMessage)> DeletePatientAsync(int id);
        Task<List<PatientNoteDto>> GetPatientNotesAsync(int id);
        Task<(bool Success, string ErrorMessage)> AddPatientNoteAsync(int id, string note);
        Task<PatientSummaryDto?> GetPatientSummaryAsync(int id);
    }

    public class PatientService : IPatientService
    {
        private readonly HttpClient _http;
        private readonly ILogger<PatientService> _logger;

        public PatientService(HttpClient http, ILogger<PatientService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<PatientDto>> SearchPatientsAsync(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query)) return new List<PatientDto>();
                var result = await _http.GetFromJsonAsync<List<PatientDto>>($"api/Patients/search?q={Uri.EscapeDataString(query)}");
                return result ?? new List<PatientDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching patients");
                return new List<PatientDto>();
            }
        }

        public async Task<List<PatientDto>> GetAllPatientsAsync()
        {
            try
            {
                var result = await _http.GetFromJsonAsync<List<PatientDto>>("api/Patients");
                return result ?? new List<PatientDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all patients");
                return new List<PatientDto>();
            }
        }

        public async Task<PatientDto?> GetPatientAsync(int id)
        {
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                };
                return await _http.GetFromJsonAsync<PatientDto>($"api/Patients/{id}", options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching patient {id}");
                return null;
            }
        }

        public async Task<(bool Success, string ErrorMessage)> CreatePatientAsync(CreatePatientDto patient)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Patients", patient);
                if (response.IsSuccessStatusCode) return (true, string.Empty);
                var error = await response.Content.ReadAsStringAsync();
                return (false, $"Failed to create patient: {response.ReasonPhrase} - {error}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating patient");
                return (false, $"Internal error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> UpdatePatientAsync(int id, UpdatePatientDto patient)
        {
            try
            {
                var response = await _http.PutAsJsonAsync($"api/Patients/{id}", patient);
                if (response.IsSuccessStatusCode) return (true, string.Empty);
                var error = await response.Content.ReadAsStringAsync();
                return (false, $"Failed to update patient: {response.ReasonPhrase} - {error}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating patient {id}");
                return (false, $"Internal error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> DeletePatientAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/Patients/{id}");
                if (response.IsSuccessStatusCode) return (true, string.Empty);
                var error = await response.Content.ReadAsStringAsync();
                return (false, $"Failed to delete patient: {response.ReasonPhrase} - {error}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting patient {id}");
                return (false, $"Internal error: {ex.Message}");
            }
        }

        public async Task<List<PatientNoteDto>> GetPatientNotesAsync(int id)
        {
            try
            {
                var result = await _http.GetFromJsonAsync<List<PatientNoteDto>>($"api/Patients/{id}/notes");
                return result ?? new List<PatientNoteDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching notes for patient {id}");
                return new List<PatientNoteDto>();
            }
        }

        public async Task<(bool Success, string ErrorMessage)> AddPatientNoteAsync(int id, string note)
        {
            try
            {
                var payload = new { text = note };
                var response = await _http.PostAsJsonAsync($"api/Patients/{id}/notes", payload);
                if (response.IsSuccessStatusCode) return (true, string.Empty);
                var error = await response.Content.ReadAsStringAsync();
                return (false, $"Failed to add note: {response.ReasonPhrase} - {error}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding note for patient {id}");
                return (false, $"Internal error: {ex.Message}");
            }
        }

        public async Task<PatientSummaryDto?> GetPatientSummaryAsync(int id)
        {
            try
            {
                var response = await _http.GetAsync($"api/Patients/{id}/summary");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<PatientSummaryDto>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching summary for patient {id}");
                return null;
            }
        }
    }
}
