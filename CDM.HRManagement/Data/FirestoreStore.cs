using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;

namespace CDM.HRManagement.Data
{
    /// <summary>
    /// Minimal Firestore wrapper for your existing POCOs (InMemoryStore.*).
    /// Uses Application Default Credentials or FIRESTORE_CREDENTIALS_JSON env var.
    /// </summary>
    public class FirestoreStore
    {
        private readonly FirestoreDb _db;

        public FirestoreStore(IConfiguration configuration)
        {
            var projectId = configuration["Firestore:ProjectId"]
                            ?? Environment.GetEnvironmentVariable("FIRESTORE_PROJECT_ID");

            if (string.IsNullOrWhiteSpace(projectId))
                throw new InvalidOperationException("Firestore project id not configured. Set Firestore:ProjectId or FIRESTORE_PROJECT_ID.");

            // If credentials JSON is provided via env var, write to temp file and set ADC env var.
            var credsJson = Environment.GetEnvironmentVariable("FIRESTORE_CREDENTIALS_JSON");
            if (!string.IsNullOrWhiteSpace(credsJson))
            {
                var tempPath = Path.Combine(Path.GetTempPath(), $"gcp-creds-{Guid.NewGuid():N}.json");
                File.WriteAllText(tempPath, credsJson);
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", tempPath);
            }

            // FirestoreDb.Create uses ADC (GOOGLE_APPLICATION_CREDENTIALS) if present
            _db = FirestoreDb.Create(projectId);
        }

        private CollectionReference ApplicantsCollection => _db.Collection("applicants");
        private CollectionReference EmployeesCollection => _db.Collection("employees");
        private CollectionReference TrainingsCollection => _db.Collection("trainings");

        // Applicants
        public async Task<List<InMemoryStore.Applicant>> GetApplicantsAsync()
        {
            var snap = await ApplicantsCollection.GetSnapshotAsync();
            var list = new List<InMemoryStore.Applicant>();
            foreach (var doc in snap.Documents)
            {
                try
                {
                    var obj = doc.ConvertTo<InMemoryStore.Applicant>();
                    if (string.IsNullOrWhiteSpace(obj.Id)) obj.Id = doc.Id;
                    list.Add(obj);
                }
                catch
                {
                    var dict = doc.ToDictionary();
                    var ap = new InMemoryStore.Applicant
                    {
                        Id = doc.Id,
                        Name = dict.TryGetValue("Name", out var n) ? n?.ToString() ?? string.Empty : string.Empty,
                        Position = dict.TryGetValue("Position", out var p) ? p?.ToString() : null,
                        Department = dict.TryGetValue("Department", out var d) ? d?.ToString() : null,
                        Email = dict.TryGetValue("Email", out var e) ? e?.ToString() : null,
                        Phone = dict.TryGetValue("Phone", out var ph) ? ph?.ToString() : null,
                        ApplicantCode = dict.TryGetValue("ApplicantCode", out var ac) ? ac?.ToString() ?? string.Empty : string.Empty,
                        Status = dict.TryGetValue("Status", out var s) ? s?.ToString() ?? "Pending Review" : "Pending Review",
                    };
                    list.Add(ap);
                }
            }
            return list;
        }

        public Task AddOrUpdateApplicantAsync(InMemoryStore.Applicant applicant)
        {
            if (string.IsNullOrWhiteSpace(applicant.Id)) applicant.Id = Guid.NewGuid().ToString();
            return ApplicantsCollection.Document(applicant.Id).SetAsync(applicant);
        }

        public Task RemoveApplicantAsync(string applicantId)
        {
            if (string.IsNullOrWhiteSpace(applicantId)) return Task.CompletedTask;
            return ApplicantsCollection.Document(applicantId).DeleteAsync();
        }

        // Employees
        public async Task<List<InMemoryStore.Employee>> GetEmployeesAsync()
        {
            var snap = await EmployeesCollection.GetSnapshotAsync();
            return snap.Documents.Select(d =>
            {
                try { return d.ConvertTo<InMemoryStore.Employee>(); }
                catch
                {
                    var dict = d.ToDictionary();
                    return new InMemoryStore.Employee
                    {
                        Id = d.Id,
                        Name = dict.TryGetValue("Name", out var n) ? n?.ToString() ?? string.Empty : string.Empty,
                        Position = dict.TryGetValue("Position", out var p) ? p?.ToString() ?? string.Empty : string.Empty,
                        Department = dict.TryGetValue("Department", out var dep) ? dep?.ToString() ?? string.Empty : string.Empty,
                        Email = dict.TryGetValue("Email", out var e) ? e?.ToString() ?? string.Empty : string.Empty,
                        Phone = dict.TryGetValue("Phone", out var ph) ? ph?.ToString() ?? string.Empty : string.Empty,
                        Status = dict.TryGetValue("Status", out var s) ? s?.ToString() ?? "Active" : "Active"
                    };
                }
            }).ToList();
        }

        public Task AddEmployeeAsync(InMemoryStore.Employee employee)
        {
            if (string.IsNullOrWhiteSpace(employee.Id)) employee.Id = Guid.NewGuid().ToString();
            return EmployeesCollection.Document(employee.Id).SetAsync(employee);
        }

        // Trainings
        public async Task<List<InMemoryStore.Training>> GetTrainingsAsync()
        {
            var snap = await TrainingsCollection.GetSnapshotAsync();
            return snap.Documents.Select(d =>
            {
                try { return d.ConvertTo<InMemoryStore.Training>(); }
                catch
                {
                    var dict = d.ToDictionary();
                    return new InMemoryStore.Training
                    {
                        Id = d.Id,
                        Department = dict.TryGetValue("Department", out var dep) ? dep?.ToString() ?? string.Empty : string.Empty,
                        Position = dict.TryGetValue("Position", out var pos) ? pos?.ToString() ?? string.Empty : string.Empty,
                        Date = dict.TryGetValue("Date", out var dt) && DateTime.TryParse(dt?.ToString(), out var parsed) ? parsed : DateTime.UtcNow.Date,
                        DurationHours = dict.TryGetValue("DurationHours", out var dur) && int.TryParse(dur?.ToString(), out var ih) ? ih : 3,
                        TrainerName = dict.TryGetValue("TrainerName", out var tn) ? tn?.ToString() ?? string.Empty : string.Empty,
                        TrainerPosition = dict.TryGetValue("TrainerPosition", out var tp) ? tp?.ToString() ?? string.Empty : string.Empty,
                    };
                }
            }).ToList();
        }

        public Task AddTrainingAsync(InMemoryStore.Training t)
        {
            if (string.IsNullOrWhiteSpace(t.Id)) t.Id = Guid.NewGuid().ToString();
            return TrainingsCollection.Document(t.Id).SetAsync(t);
        }

        // Hire: move applicant -> employee
        public async Task<bool> HireApplicantAsync(string applicantId)
        {
            if (string.IsNullOrWhiteSpace(applicantId)) return false;
            var docRef = ApplicantsCollection.Document(applicantId);
            var snap = await docRef.GetSnapshotAsync();
            if (!snap.Exists) return false;

            var app = snap.ConvertTo<InMemoryStore.Applicant>();
            var emp = new InMemoryStore.Employee
            {
                Id = Guid.NewGuid().ToString(),
                Name = app.Name,
                Position = app.Position ?? string.Empty,
                Department = app.Department ?? string.Empty,
                Email = app.Email ?? string.Empty,
                Phone = app.Phone ?? string.Empty,
                StartDate = DateTime.UtcNow.Date,
                Status = "Active"
            };

            await AddEmployeeAsync(emp);
            await docRef.DeleteAsync();
            return true;
        }

        // Migrate InMemory -> Firestore (call explicitly)
        public async Task MigrateFromInMemoryAsync()
        {
            foreach (var a in InMemoryStore.Applicants.ToList())
            {
                if (string.IsNullOrWhiteSpace(a.Id)) a.Id = Guid.NewGuid().ToString();
                await ApplicantsCollection.Document(a.Id).SetAsync(a);
            }

            foreach (var e in InMemoryStore.Employees.ToList())
            {
                if (string.IsNullOrWhiteSpace(e.Id)) e.Id = Guid.NewGuid().ToString();
                await EmployeesCollection.Document(e.Id).SetAsync(e);
            }

            foreach (var t in InMemoryStore.Trainings.ToList())
            {
                if (string.IsNullOrWhiteSpace(t.Id)) t.Id = Guid.NewGuid().ToString();
                await TrainingsCollection.Document(t.Id).SetAsync(t);
            }
        }
    }
}