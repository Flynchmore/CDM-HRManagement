using System;
using System.Collections.Generic;

namespace CDM.HRManagement.Data
{
    public static class InMemoryStore
    {
        public static List<Employee> Employees { get; set; } = new List<Employee>();
        public static List<Training> Trainings { get; set; } = new List<Training>();
        public static List<Applicant> Applicants { get; set; } = new List<Applicant>();

        public static string GenerateApplicantId()
        {
            // Format: APP-YYYYMMDD-HHMMSS-XXX
            return $"APP-{DateTime.Now:yyyyMMdd-HHmmss}-{Applicants.Count + 1:D3}";
        }

        // Example method for employee IDs
        public static string GenerateEmployeeId()
        {
            return $"EMP-{DateTime.Now:yyyyMMdd-HHmmss}-{Employees.Count + 1:D3}";
        }

        public class Employee
        {
            public string Id { get; set; } = string.Empty; // CDM-001
            public string Name { get; set; } = string.Empty;
            public string Position { get; set; } = string.Empty; // 👈 ADD THIS LINE
            public string Department { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;
            public string Status { get; set; } = "Active";
        }

        public class Training
        {
            public string Id { get; set; } = string.Empty;
            public string Department { get; set; } = string.Empty;
            public string Position { get; set; } = string.Empty;
            public DateTime Date { get; set; } = DateTime.UtcNow.Date;
            public int DurationHours { get; set; } = 3;
            public string TrainerName { get; set; } = string.Empty;
            public string TrainerPosition { get; set; } = string.Empty;
            public List<string> Participants { get; set; } = new List<string>();
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        }
        public class Applicant
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string? Position { get; set; }
            public string? Department { get; set; }
            public string? Email { get; set; }
            public string? Phone { get; set; }
            public string ApplicantCode { get; set; } = string.Empty;
            public string Status { get; set; } = "Pending Review";
            public DateTime Submitted { get; set; } = DateTime.Now;
            public OnboardingInfo? OnboardingData { get; set; }
        }

        public class OnboardingInfo
        {
            // Personal
            public string? LastName { get; set; }
            public string? FirstName { get; set; }
            public string? MiddleName { get; set; }
            public string? Sex { get; set; }
            public string? CivilStatus { get; set; }
            public double? Height { get; set; }
            public double? Weight { get; set; }

            // Birth
            public DateTime? BirthDate { get; set; }
            public string? BirthPlace { get; set; }

            // Contact
            public string? Email { get; set; }
            public string? Phone { get; set; }
            public string? Address { get; set; }

            // Family
            public string? FatherFullName { get; set; }
            public string? FatherOccupation { get; set; }
            public string? MotherMaidenName { get; set; }
            public string? MotherOccupation { get; set; }
            public string? ChildrenSummary { get; set; }

            // Position
            public string? Position { get; set; }
            public string? Department { get; set; }

            // Education
            public string? ElementarySchool { get; set; }
            public string? ElementaryYear { get; set; }
            public string? JuniorHighSchool { get; set; }
            public string? JuniorHighYear { get; set; }
            public string? SeniorHighSchool { get; set; }
            public string? SeniorHighYear { get; set; }
            public string? CollegeSchool { get; set; }
            public string? CollegeDegree { get; set; }
            public string? CollegeYear { get; set; }
            public string? GradSchool { get; set; }
            public string? GradDegree { get; set; }
            public string? GradYear { get; set; }

            // Work / Training
            public object? WorkExperience { get; set; }
            public object? Trainings { get; set; }
            public string? Skills { get; set; }
            public string? Hobbies { get; set; }
        }
    }
}
