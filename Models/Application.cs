namespace InternshipManagementSystem.Models
{
    public class Application
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int InternshipId { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime AppliedAt { get; set; } = DateTime.Now;

        // These properties are for the JOIN query results
        public string? InternshipTitle { get; set; }
        public string? InternshipLocation { get; set; }
        public string? StudentName { get; set; }
        public string? StudentEmail { get; set; }
        public string? ResumeFileName { get; set; }
        public string? Feedback { get; set; }

        // ✅ NEW: To show which company posted the internship
        public string? CompanyName { get; set; }
    }
}