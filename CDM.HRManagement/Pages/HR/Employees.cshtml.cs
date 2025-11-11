using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CDM.HRManagement.Data;

namespace CDM.HRManagement.Pages.HR
{
    public class EmployeesModel : PageModel
    {
        [TempData]
        public string? SuccessMessage { get; set; }

        public List<InMemoryStore.Employee> Employees { get; set; } = new List<InMemoryStore.Employee>();

        // Bound model for edit form
        [BindProperty]
        public EditEmployeeInput EditEmployee { get; set; } = new EditEmployeeInput();

        public void OnGet()
        {
            Employees = InMemoryStore.Employees.ToList();
        }

        // Delete handler
        public IActionResult OnPostDeleteEmployee(string employeeId)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
            {
                SuccessMessage = "Invalid employee id.";
                return RedirectToPage();
            }

            var removed = InMemoryStore.Employees.RemoveAll(e => e.Id == employeeId);
            SuccessMessage = removed > 0 ? "Employee deleted." : "Employee not found.";
            return RedirectToPage();
        }

        // Edit handler
        public IActionResult OnPostEditEmployee()
        {
            if (string.IsNullOrWhiteSpace(EditEmployee.Id))
            {
                SuccessMessage = "Invalid employee id.";
                return RedirectToPage();
            }

            var emp = InMemoryStore.Employees.FirstOrDefault(e => e.Id == EditEmployee.Id);
            if (emp == null)
            {
                SuccessMessage = "Employee not found.";
                return RedirectToPage();
            }

            // Update allowed fields only
            emp.Name = EditEmployee.Name ?? emp.Name;
            emp.Email = EditEmployee.Email ?? emp.Email;
            emp.Status = EditEmployee.Status ?? emp.Status;

            SuccessMessage = $"Employee {emp.Id} updated.";
            return RedirectToPage();
        }

        public class EditEmployeeInput
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
        }
    }
}