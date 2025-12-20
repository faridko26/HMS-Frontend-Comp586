using HMS_Frontend.Models;
using HMS_Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace HMS_Frontend.Pages.Appointments
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IApiService _api;

        public IndexModel(IApiService api)
        {
            _api = api;
        }

        public List<AppointmentDto> Appointments { get; set; } = new();

        [BindProperty]
        public CreateAppointmentDto NewAppointment { get; set; } = new();

        [BindProperty]
        public UpdateAppointmentDto EditAppointment { get; set; } = new();

        [BindProperty]
        public int EditAppointmentId { get; set; }

        public List<PatientDto> Patients { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Determine PST
            TimeZoneInfo pstZone;
            try
            {
                pstZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");
            }
            catch (TimeZoneNotFoundException)
            {
                pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            }

            var nowPst = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pstZone);
            NewAppointment.StartsAt = new DateTime(nowPst.Year, nowPst.Month, nowPst.Day, nowPst.Hour, nowPst.Minute, 0);
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            // Fetch all appointments (passing no date fetches all)
            Appointments = await _api.GetAppointmentsAsync();

            // Sort by date descending (newest first)
            Appointments = Appointments.OrderByDescending(a => a.StartsAt).ToList();

            // Convert to PST for display
            TimeZoneInfo pstZone;
            try
            {
                pstZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");
            }
            catch (TimeZoneNotFoundException)
            {
                pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            }

            foreach (var appt in Appointments)
            {
                appt.StartsAt = TimeZoneInfo.ConvertTime(appt.StartsAt, pstZone);
            }

            // Fetch patients for dropdown
            Patients = await _api.GetAllPatientsAsync();

            // Hydrate patients if missing (though API usually returns them)
            foreach (var appt in Appointments)
            {
                if (appt.Patient == null && appt.PatientId > 0)
                {
                    appt.Patient = await _api.GetPatientAsync(appt.PatientId);
                }
            }
        }

        public async Task<IActionResult> OnPostAddAppointmentAsync()
        {
            // Force UTC/Zero offset for timezone agnostic handling
            NewAppointment.StartsAt = new DateTimeOffset(NewAppointment.StartsAt.DateTime, TimeSpan.Zero);

            var result = await _api.CreateAppointmentAsync(NewAppointment);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Appointment scheduled successfully.";
                return RedirectToPage();
            }

            ModelState.AddModelError("", result.ErrorMessage);
            await LoadDataAsync();
            ViewData["ShowAddModal"] = true;
            return Page();
        }

        public async Task<IActionResult> OnPostCancelAppointmentAsync(int id)
        {
            var result = await _api.CancelAppointmentAsync(id);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Appointment cancelled successfully.";
            }
            else
            {
                ModelState.AddModelError("", result.ErrorMessage);
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAppointmentAsync()
        {
            if (!ModelState.IsValid)
            {
                // Reload data if validation fails
                await LoadDataAsync();
                ViewData["ShowEditModal"] = true;
                return Page();
            }

            // Force UTC/Zero offset for timezone agnostic handling
            EditAppointment.StartsAt = new DateTimeOffset(EditAppointment.StartsAt.DateTime, TimeSpan.Zero);
            EditAppointment.Id = EditAppointmentId;

            var result = await _api.UpdateAppointmentAsync(EditAppointmentId, EditAppointment);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Appointment updated successfully.";
                return RedirectToPage();
            }

            ModelState.AddModelError("", result.ErrorMessage);
            await LoadDataAsync();
            ViewData["ShowEditModal"] = true;
            return Page();
        }
    }
}
