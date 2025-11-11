using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CDM.HRManagement.Data;

namespace CDM.HRManagement.Pages.HR
{
    public class TrainingsModel : PageModel
    {
        // keep the same department -> positions mapping used elsewhere
        public static Dictionary<string, List<string>> DepartmentPositions { get; } = new Dictionary<string, List<string>>
        {
            { "Institute of Computing Studies", new List<string> { "Professor", "IT Technician", "Laboratory Assistant" } },
            { "Institute of Business", new List<string> { "Accountant", "Business Professor", "HR Officer" } },
            { "Institute of Education", new List<string> { "Professor", "Guidance Counselor", "Research Coordinator" } }
        };

        public List<InMemoryStore.Training> Trainings { get; set; } = new List<InMemoryStore.Training>();

        [TempData]
        public string? SuccessMessage { get; set; }

        [BindProperty]
        public AddTrainingInput NewTraining { get; set; } = new AddTrainingInput();

        [BindProperty]
        public EditTrainingInput EditTraining { get; set; } = new EditTrainingInput();

        public void OnGet()
        {
            Trainings = InMemoryStore.Trainings.OrderByDescending(t => t.Date).ToList();
        }

        public IActionResult OnPostAddTraining()
        {
            if (string.IsNullOrWhiteSpace(NewTraining.Department) || string.IsNullOrWhiteSpace(NewTraining.Position))
            {
                SuccessMessage = "Department and Position are required.";
                return RedirectToPage();
            }

            var training = new InMemoryStore.Training
            {
                Id = Guid.NewGuid().ToString(),
                Department = NewTraining.Department,
                Position = NewTraining.Position,
                Date = NewTraining.Date,
                DurationHours = NewTraining.DurationHours,
                TrainerName = NewTraining.TrainerName?.Trim() ?? string.Empty,
                TrainerPosition = NewTraining.TrainerPosition?.Trim() ?? string.Empty,
                Participants = SplitParticipantsText(NewTraining.ParticipantsText),
                CreatedAt = DateTime.Now
            };

            InMemoryStore.Trainings.Add(training);
            SuccessMessage = "Training added.";
            return RedirectToPage();
        }

        public IActionResult OnPostDeleteTraining(string trainingId)
        {
            if (string.IsNullOrWhiteSpace(trainingId))
            {
                SuccessMessage = "Invalid training id.";
                return RedirectToPage();
            }

            var removed = InMemoryStore.Trainings.RemoveAll(t => t.Id == trainingId);
            SuccessMessage = removed > 0 ? "Training deleted." : "Training not found.";
            return RedirectToPage();
        }

        public IActionResult OnPostEditTraining()
        {
            if (string.IsNullOrWhiteSpace(EditTraining.Id))
            {
                SuccessMessage = "Invalid training id.";
                return RedirectToPage();
            }

            var t = InMemoryStore.Trainings.FirstOrDefault(x => x.Id == EditTraining.Id);
            if (t == null)
            {
                SuccessMessage = "Training not found.";
                return RedirectToPage();
            }

            // Only allowed editable fields: DurationHours, TrainerName, TrainerPosition, Participants
            t.DurationHours = EditTraining.DurationHours;
            t.TrainerName = EditTraining.TrainerName?.Trim() ?? t.TrainerName;
            t.TrainerPosition = EditTraining.TrainerPosition?.Trim() ?? t.TrainerPosition;
            t.Participants = SplitParticipantsText(EditTraining.ParticipantsText);

            SuccessMessage = "Training updated.";
            return RedirectToPage();
        }

        private static List<string> SplitParticipantsText(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            var lines = text
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            return lines;
        }

        public class AddTrainingInput
        {
            public string Department { get; set; } = string.Empty;
            public string Position { get; set; } = string.Empty;
            public DateTime Date { get; set; } = DateTime.UtcNow.Date;
            public int DurationHours { get; set; } = 3;
            public string TrainerName { get; set; } = string.Empty;
            public string TrainerPosition { get; set; } = string.Empty;
            public string ParticipantsText { get; set; } = string.Empty; // one name per line
        }

        public class EditTrainingInput
        {
            public string Id { get; set; } = string.Empty;
            public int DurationHours { get; set; } = 3;
            public string TrainerName { get; set; } = string.Empty;
            public string TrainerPosition { get; set; } = string.Empty;
            public string ParticipantsText { get; set; } = string.Empty;
        }
    }
}