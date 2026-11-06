namespace InternshipManagementSystem.Models
{
    public class CV
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string? LinkedIn { get; set; }
        public string Objective { get; set; } = string.Empty;
        public string Education { get; set; } = string.Empty;
        public string Experience { get; set; } = string.Empty;
        public string? PhotoFileName { get; set; }
        public string? Skills { get; set; }
        public string? Interests { get; set; }

        // To hold student info for the view
        public string? StudentName { get; set; }
        public string? StudentEmail { get; set; }
    }
}