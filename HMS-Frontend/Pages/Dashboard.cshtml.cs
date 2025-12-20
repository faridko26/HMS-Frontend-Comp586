using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HMS_Frontend.Services;
using HMS_Frontend.Models;

namespace HMS_Frontend.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly IApiService _api;

        public DashboardModel(IApiService api)
        {
            _api = api;
        }

        public List<AppointmentDto> Appointments { get; set; } = new();
        public List<PatientDto> SearchResults { get; set; } = new();

        [BindProperty]
        public CreatePatientDto Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        [BindProperty]
        public CreateAppointmentDto NewAppointment { get; set; } = new();

        [BindProperty]
        public UpdateAppointmentDto EditAppointment { get; set; } = new();

        [BindProperty]
        public int EditAppointmentId { get; set; }

        public List<StaffUserDto> StaffMembers { get; set; } = new();
        public StaffUserDto? CurrentUser { get; set; }

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
            Input.DateOfBirth = DateTime.Today;
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            // Fetch ALL appointments (pass null to get everything)
            var allAppointments = await _api.GetAppointmentsAsync(null);

            // Determine "Today" in Pacific Time (PST/PDT)
            TimeZoneInfo pstZone;
            try
            {
                // Try Linux/Mac ID first (Render uses Linux)
                pstZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");
            }
            catch (TimeZoneNotFoundException)
            {
                // Fallback to Windows ID
                pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            }

            var todayPst = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pstZone).Date;

            // Filter appointments where the Face Value date matches Today in PST
            Appointments = allAppointments
                .Where(a => a.StartsAt.DateTime.Date == todayPst)
                .OrderBy(a => a.StartsAt)
                .ToList();

            // Convert UTC times to PST for display
            foreach (var appt in Appointments)
            {
                appt.StartsAt = TimeZoneInfo.ConvertTime(appt.StartsAt, pstZone);
            }

            StaffMembers = await _api.GetStaffAsync();

            var username = User.Identity?.Name;

            if (!string.IsNullOrEmpty(username))
            {
                CurrentUser = StaffMembers.FirstOrDefault(s => s.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            }

            // Hydrate missing patients
            foreach (var appt in Appointments)
            {
                if (appt.Patient == null && appt.PatientId > 0)
                {
                    appt.Patient = await _api.GetPatientAsync(appt.PatientId);
                }
            }

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                SearchResults = await _api.SearchPatientsAsync(SearchQuery);
                // Hydrate appointments for search results if missing
                for (int i = 0; i < SearchResults.Count; i++)
                {
                    var summary = await _api.GetPatientSummaryAsync(SearchResults[i].Id);
                    if (summary != null)
                    {
                        // Map summary appointments to PatientDto appointments
                        SearchResults[i].Appointments = summary.Appointments.Select(a => new AppointmentDto
                        {
                            Id = a.AppointmentId,
                            StartsAt = a.StartsAt,
                            DurationMinutes = a.DurationMinutes,
                            Reason = a.Reason,
                            Status = a.Status,
                            StaffUserId = a.StaffUserId,
                            PatientId = summary.PatientId
                        }).ToList();
                    }
                }
            }
        }

        public async Task<IActionResult> OnPostAddPatientAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            var result = await _api.CreatePatientAsync(Input);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Patient created successfully!";
                return RedirectToPage();
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                await LoadDataAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostScheduleAppointmentAsync()
        {
            // The input comes in as "Face Value" (e.g. 2:13 PM) but with Server's Offset (UTC on Render).
            // We need to interpret this Face Value as PST, then convert to UTC.
            var faceValue = NewAppointment.StartsAt.DateTime;

            TimeZoneInfo pstZone;
            try
            {
                pstZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");
            }
            catch (TimeZoneNotFoundException)
            {
                pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            }

            // Create offset for this time in PST
            var pstOffset = pstZone.GetUtcOffset(faceValue);
            // Create DateTimeOffset with PST offset
            var pstTime = new DateTimeOffset(faceValue, pstOffset);

            // Convert to UTC for storage
            NewAppointment.StartsAt = pstTime.ToUniversalTime();

            var result = await _api.CreateAppointmentAsync(NewAppointment);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Appointment scheduled successfully!";
                return RedirectToPage();
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                await LoadDataAsync();
                return Page();
            }
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
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAppointmentAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                ViewData["ShowEditModal"] = true;
                return Page();
            }

            // The input comes in as "Face Value" (e.g. 2:13 PM) but with Server's Offset (UTC on Render).
            // We need to interpret this Face Value as PST, then convert to UTC.
            var faceValue = EditAppointment.StartsAt.DateTime;

            TimeZoneInfo pstZone;
            try
            {
                pstZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");
            }
            catch (TimeZoneNotFoundException)
            {
                pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            }

            // Create offset for this time in PST
            var pstOffset = pstZone.GetUtcOffset(faceValue);
            // Create DateTimeOffset with PST offset
            var pstTime = new DateTimeOffset(faceValue, pstOffset);

            // Convert to UTC for storage
            EditAppointment.StartsAt = pstTime.ToUniversalTime();

            // Ensure ID is set in the body as well
            EditAppointment.Id = EditAppointmentId;

            var result = await _api.UpdateAppointmentAsync(EditAppointmentId, EditAppointment);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Appointment updated successfully!";
                return RedirectToPage();
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                await LoadDataAsync();
                ViewData["ShowEditModal"] = true;
                return Page();
            }
        }
    }
}
