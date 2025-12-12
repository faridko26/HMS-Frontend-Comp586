using HMS_Frontend.Models;
using HMS_Frontend.Services.Domain;

namespace HMS_Frontend.Services.Strategies
{
    public interface IPatientSearchStrategy
    {
        Task<List<PatientDto>> SearchAsync(string query, IPatientService patientService);
    }

    public class NameSearchStrategy : IPatientSearchStrategy
    {
        public async Task<List<PatientDto>> SearchAsync(string query, IPatientService patientService)
        {
            // Delegates to the service's API search which is likely name-based
            return await patientService.SearchPatientsAsync(query);
        }
    }

    public class IdSearchStrategy : IPatientSearchStrategy
    {
        public async Task<List<PatientDto>> SearchAsync(string query, IPatientService patientService)
        {
            if (int.TryParse(query, out int id))
            {
                var patient = await patientService.GetPatientAsync(id);
                return patient != null ? new List<PatientDto> { patient } : new List<PatientDto>();
            }
            return new List<PatientDto>();
        }
    }
}
