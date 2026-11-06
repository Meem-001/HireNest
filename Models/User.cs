namespace InternshipManagementSystem.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Student";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // ✅ Skills (used for matching)
        public string? Skills { get; set; }
        public int PostsRemaining { get; set; } = 2;

        // ✅ NEW: Contact Information
        public string? Phone { get; set; }
        public string? LinkedIn { get; set; }
        public string? GitHub { get; set; }
        public string? Portfolio { get; set; }

        // ✅ NEW: Student-Specific Fields
        public string? Bio { get; set; }
        public string? Education { get; set; }
        public string? WorkPreference { get; set; } // Remote, On-site, Hybrid
        public string? Availability { get; set; } // When can start

        // ✅ NEW: Company-Specific Fields
        public string? Website { get; set; }
        public string? CompanyDescription { get; set; }
        public string? Industry { get; set; }
        public string? Location { get; set; }
    }
}