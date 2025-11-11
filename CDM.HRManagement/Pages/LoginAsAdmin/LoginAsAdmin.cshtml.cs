using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CDM.HRManagement.Pages
{
    public class LoginAsAdminModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (Username == "cdm_hradmin" && Password == "1234")
            {
                return RedirectToPage("/HR/Dashboard");
            }
            ErrorMessage = "Incorrect username or password. Please try again.";
            return Page();
        }
    }
}