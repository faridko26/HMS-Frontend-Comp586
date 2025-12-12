using HMS_Frontend.Models;
using HMS_Frontend.Services.Domain;
using HMS_Frontend.Services.Strategies;

namespace HMS_Frontend.Services
{
    // The Interface remains the same to support existing code
    public interface IApiService
    {
        Task<List<AppointmentDto>> GetAppointmentsAsync(DateTime? date = null);
        Task<List<PatientDto>> SearchPatientsAsync(string query);
        Task<List<PatientDto>> GetAllPatientsAsync();
        Task<PatientDto?> GetPatientAsync(int id);
        Task<(bool Success, string ErrorMessage)> CreatePatientAsync(CreatePatientDto patient);
        Task<(bool Success, string ErrorMessage)> CreateAppointmentAsync(CreateAppointmentDto appointment);
        Task<(bool Success, string ErrorMessage)> CancelAppointmentAsync(int id);
        Task<(bool Success, string ErrorMessage)> UpdateAppointmentAsync(int id, UpdateAppointmentDto appointment);
        Task<List<StaffUserDto>> GetDoctorsAsync();
        Task<List<PatientNoteDto>> GetPatientNotesAsync(int id);
        Task<(bool Success, string ErrorMessage)> AddPatientNoteAsync(int id, string note);
        Task<PatientSummaryDto?> GetPatientSummaryAsync(int id);
        Task<(bool Success, string ErrorMessage)> UpdatePatientAsync(int id, UpdatePatientDto patient);
        Task<(bool Success, string ErrorMessage)> DeletePatientAsync(int id);
        Task<List<StaffUserDto>> GetStaffAsync();
        Task<(bool Success, string ErrorMessage)> UpdateUserAsync(UserAccountDto user);

        // New method to set strategy
        void SetSearchStrategy(IPatientSearchStrategy strategy);
    }

    public class ApiService : IApiService
    {
        private readonly IPatientService _patientService;
        private readonly IAppointmentService _appointmentService;
        private readonly IStaffService _staffService;

        private IPatientSearchStrategy _searchStrategy;

        // Constructor Injection for the Sub-systems (Facade Pattern)
        public ApiService(HttpClient http, ILogger<ApiService> logger)
        {
            // In a real DI scenario, these would be injected directly.
            // For this refactor, we initialize them here using the same HttpClient
            // to maintain the existing Program.cs structure where ApiService is registered with HttpClient.

            // We create loggers manually or could inject ILoggerFactory. 
            // For simplicity in this refactor step, we'll use the main logger or create wrappers.
            // Ideally, we'd update DI to inject IPatientService etc. directly into ApiService.

            var patientLogger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<PatientService>();
            var apptLogger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<AppointmentService>();
            var staffLogger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<StaffService>();

            _patientService = new PatientService(http, patientLogger);
            _appointmentService = new AppointmentService(http, apptLogger);
            _staffService = new StaffService(http, staffLogger);

            // Default Strategy
            _searchStrategy = new NameSearchStrategy();
        }

        public void SetSearchStrategy(IPatientSearchStrategy strategy)
        {
            _searchStrategy = strategy;
        }

        // ===== Facade Methods Delegating to Sub-Systems =====

        // Appointments -> AppointmentService
        public Task<List<AppointmentDto>> GetAppointmentsAsync(DateTime? date = null)
            => _appointmentService.GetAppointmentsAsync(date);

        public Task<(bool Success, string ErrorMessage)> CreateAppointmentAsync(CreateAppointmentDto appointment)
            => _appointmentService.CreateAppointmentAsync(appointment);

        public Task<(bool Success, string ErrorMessage)> CancelAppointmentAsync(int id)
            => _appointmentService.CancelAppointmentAsync(id);

        public Task<(bool Success, string ErrorMessage)> UpdateAppointmentAsync(int id, UpdateAppointmentDto appointment)
            => _appointmentService.UpdateAppointmentAsync(id, appointment);

        // Patients -> PatientService (and Strategy)
        public Task<List<PatientDto>> SearchPatientsAsync(string query)
        {
            // Use the Strategy
            return _searchStrategy.SearchAsync(query, _patientService);
        }

        public Task<List<PatientDto>> GetAllPatientsAsync() => _patientService.GetAllPatientsAsync();
        public Task<PatientDto?> GetPatientAsync(int id) => _patientService.GetPatientAsync(id);
        public Task<(bool Success, string ErrorMessage)> CreatePatientAsync(CreatePatientDto patient) => _patientService.CreatePatientAsync(patient);
        public Task<(bool Success, string ErrorMessage)> UpdatePatientAsync(int id, UpdatePatientDto patient) => _patientService.UpdatePatientAsync(id, patient);
        public Task<(bool Success, string ErrorMessage)> DeletePatientAsync(int id) => _patientService.DeletePatientAsync(id);
        public Task<List<PatientNoteDto>> GetPatientNotesAsync(int id) => _patientService.GetPatientNotesAsync(id);
        public Task<(bool Success, string ErrorMessage)> AddPatientNoteAsync(int id, string note) => _patientService.AddPatientNoteAsync(id, note);
        public Task<PatientSummaryDto?> GetPatientSummaryAsync(int id) => _patientService.GetPatientSummaryAsync(id);

        // Staff -> StaffService
        public Task<List<StaffUserDto>> GetDoctorsAsync() => _staffService.GetDoctorsAsync();
        public Task<List<StaffUserDto>> GetStaffAsync() => _staffService.GetStaffAsync();
        public Task<(bool Success, string ErrorMessage)> UpdateUserAsync(UserAccountDto user) => _staffService.UpdateUserAsync(user);
    }
}
