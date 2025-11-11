using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CDM.HRManagement.Data;
using System.Linq;

namespace CDM.HRManagement.Pages.Onboarding
{
    public class FormModel : PageModel
    {
        [BindProperty]
        public InMemoryStore.OnboardingInfo OnboardingData { get; set; } = new();


        [BindProperty(SupportsGet = true)]
        public string? Token { get; set; }


        public IActionResult OnGet(string? token)
        {
            Token = token;
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            var applicant = InMemoryStore.Applicants.FirstOrDefault(a => a.Id == Token);
            if (applicant != null)
            {
                applicant.OnboardingData = OnboardingData;
                applicant.Status = "Onboarding Submitted";
            }

            return RedirectToPage("/Onboarding/ThankYou");
        }
    }
}
