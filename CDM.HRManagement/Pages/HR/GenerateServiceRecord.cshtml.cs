using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CDM.HRManagement.Data;

namespace CDM.HRManagement.Pages.HR
{
    public class GenerateServiceRecordModel : PageModel
    {
        // Only active employees will be exposed to the view
        public List<InMemoryStore.Employee> Employees { get; set; } = new List<InMemoryStore.Employee>();

        public void OnGet()
        {
            // Filter the shared store: only Active employees are selectable for service records
            Employees = InMemoryStore.Employees
                .Where(e => string.Equals(e.Status, "Active", System.StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}