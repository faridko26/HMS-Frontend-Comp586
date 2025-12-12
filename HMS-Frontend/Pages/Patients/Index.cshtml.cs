using HMS_Frontend.Models;
using HMS_Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HMS_Frontend.Pages.Patients
{
    public class IndexModel : PageModel
    {
        private readonly IApiService _api;

        public IndexModel(IApiService api)
        {
            _api = api;
        }

        public List<PatientDto> Patients { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        [BindProperty]
        public CreatePatientDto NewPatient { get; set; } = new();

        [BindProperty]
        public UpdatePatientDto EditPatient { get; set; } = new();

        [BindProperty]
        public int EditPatientId { get; set; }

        [BindProperty]
        public string NewNoteText { get; set; } = string.Empty;

        [BindProperty]
        public int NotePatientId { get; set; }

        public List<PatientNoteDto> SelectedPatientNotes { get; set; } = new();
        public PatientSummaryDto? SelectedPatientSummary { get; set; }
        public int? SelectedPatientId { get; set; }

        public async Task OnGetAsync(int? patientId = null)
        {
            NewPatient.DateOfBirth = DateTime.Today;
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                Patients = await _api.SearchPatientsAsync(SearchQuery);
            }
            else
            {
                Patients = await _api.GetAllPatientsAsync();
            }

            if (patientId.HasValue)
            {
                SelectedPatientId = patientId;
                SelectedPatientNotes = await _api.GetPatientNotesAsync(patientId.Value);
                SelectedPatientSummary = await _api.GetPatientSummaryAsync(patientId.Value);
            }
        }

        public async Task<IActionResult> OnPostAddPatientAsync()
        {
            // Clear errors for other forms
            ModelState.Remove(nameof(NewNoteText));
            ModelState.Remove(nameof(EditPatientId));
            // We can't easily remove all sub-properties of EditPatient, but we can iterate keys
            foreach (var key in ModelState.Keys.Where(k => k.StartsWith("EditPatient")).ToList())
            {
                ModelState.Remove(key);
            }

            if (!ModelState.IsValid) return Page();

            var result = await _api.CreatePatientAsync(NewPatient);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Patient created successfully.";
                return RedirectToPage();
            }

            TempData["ErrorMessage"] = result.ErrorMessage;
            return Page();
        }

        public async Task<IActionResult> OnPostEditPatientAsync()
        {
            // Clear errors for other forms
            ModelState.Remove(nameof(NewNoteText));
            foreach (var key in ModelState.Keys.Where(k => k.StartsWith("NewPatient")).ToList())
            {
                ModelState.Remove(key);
            }

            if (!ModelState.IsValid) return Page();
            var result = await _api.UpdatePatientAsync(EditPatientId, EditPatient);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Patient updated successfully.";
                return RedirectToPage(new { SearchQuery });
            }

            TempData["ErrorMessage"] = result.ErrorMessage;
            return Page();
        }

        public async Task<IActionResult> OnPostDeletePatientAsync(int id)
        {
            var result = await _api.DeletePatientAsync(id);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Patient deleted successfully.";
                return RedirectToPage(new { SearchQuery });
            }

            TempData["ErrorMessage"] = result.ErrorMessage;
            return Page();
        }

        public async Task<IActionResult> OnPostAddNoteAsync()
        {
            if (string.IsNullOrWhiteSpace(NewNoteText)) return RedirectToPage(new { patientId = NotePatientId, SearchQuery });

            var result = await _api.AddPatientNoteAsync(NotePatientId, NewNoteText);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Note added successfully.";
                return RedirectToPage(new { patientId = NotePatientId, SearchQuery });
            }

            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToPage(new { patientId = NotePatientId, SearchQuery });
        }
    }
}
