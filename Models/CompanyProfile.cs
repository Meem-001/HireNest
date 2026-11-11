namespace InternshipManagementSystem.Models
{
    public class CompanyProfile
    {
        public int Id { get; set; }
        public int CompanyId { get; set; } // Links to Users.Id
        public int PostsRemaining { get; set; } = 2;
        public string? Website { get; set; }
        public string? CompanyDescription { get; set; }
        public string? Industry { get; set; }
        public string? Location { get; set; }
        public string? PackageType { get; set; } = "Free"; // Free, Premium, Enterprise
        public DateTime? PackageExpiryDate { get; set; }
    }
}