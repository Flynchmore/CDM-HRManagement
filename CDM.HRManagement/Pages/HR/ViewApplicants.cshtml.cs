using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CDM.HRManagement.Data;

namespace CDM.HRManagement.Pages.HR
{
    public class ViewApplicantsModel : PageModel
    {
        public static Dictionary<string, List<string>> DepartmentPositions { get; } = new Dictionary<string, List<string>>
        {
            { "Institute of Computing Studies", new List<string> { "Professor", "IT Technician", "Laboratory Assistant" } },
            { "Institute of Business", new List<string> { "Accountant", "Business Professor", "HR Officer" } },
            { "Institute of Education", new List<string> { "Professor", "Guidance Counselor", "Research Coordinator" } }
        };

        private readonly FirestoreStore _store;

        public ViewApplicantsModel(FirestoreStore store)
        {
            _store = store;
        }

        [BindProperty]
        public string? GeneratedLink { get; set; }

        public List<InMemoryStore.Applicant> Applicants { get; set; } = new List<InMemoryStore.Applicant>();
        public List<string> Departments => DepartmentPositions.Keys.ToList();

        [BindProperty]
        public InMemoryStore.Applicant NewApplicant { get; set; } = new InMemoryStore.Applicant();

        [TempData]
        public string? SuccessMessage { get; set; }

        public async Task OnGetAsync()
        {
            Applicants = await _store.GetApplicantsAsync();
        }

        public IActionResult OnPostGenerateLink(string applicantId)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}/Onboarding/Form";
            var link = $"{baseUrl}?token={applicantId}";

            TempData["GeneratedLink"] = link;
            TempData["GeneratedLinkFor"] = applicantId;
            GeneratedLink = link;

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddApplicantAsync()
        {
            if (!ModelState.IsValid)
            {
                Applicants = await _store.GetApplicantsAsync();
                return Page();
            }

            NewApplicant.Id = Guid.NewGuid().ToString();
            NewApplicant.ApplicantCode = InMemoryStore.GenerateApplicantId();
            NewApplicant.Submitted = DateTime.Now;
            NewApplicant.Status = "Pending Review";

            await _store.AddOrUpdateApplicantAsync(NewApplicant);

            SuccessMessage = "Applicant added successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostHireApplicantAsync(string applicantId)
        {
            if (string.IsNullOrWhiteSpace(applicantId))
            {
                SuccessMessage = "Invalid applicant id.";
                return RedirectToPage();
            }

            var ok = await _store.HireApplicantAsync(applicantId);
            SuccessMessage = ok ? "Applicant hired." : "Applicant not found.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectApplicantAsync(string applicantId)
        {
            if (string.IsNullOrWhiteSpace(applicantId))
            {
                SuccessMessage = "Invalid applicant id.";
                return RedirectToPage();
            }

            await _store.RemoveApplicantAsync(applicantId);
            SuccessMessage = "Applicant rejected.";
            return RedirectToPage();
        }

        public JsonResult OnGetPositions(string department)
        {
            if (DepartmentPositions.ContainsKey(department))
                return new JsonResult(DepartmentPositions[department]);
            return new JsonResult(new List<string>());
        }
    }
}