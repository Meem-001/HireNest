namespace InternshipManagementSystem.Models
{
    public class InternshipDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string RequiredSkills { get; set; }
        public string Location { get; set; }
        public string Duration { get; set; }
        public int CompanyId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CompanyName { get; set; }
        public string CompanyEmail { get; set; }

        // ✅ ADD THESE 3 LINES:
        public string? Salary { get; set; }
        public string? JobType { get; set; }
        public DateTime? ClosingDate { get; set; }
    }
}