using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

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
        public string? Skills { get; set; }
        public string? Phone { get; set; }
        public string? LinkedIn { get; set; }
        public string? GitHub { get; set; }
        public string? Portfolio { get; set; }
        public string? Bio { get; set; }
        public string? Education { get; set; }
        public string? WorkPreference { get; set; }
        public string? Availability { get; set; }

        // 🚫 Tell EF these are NOT in the 'users' table
        [NotMapped] public int PostsRemaining { get; set; } = 2;
        [NotMapped] public string? Website { get; set; }
        [NotMapped] public string? CompanyDescription { get; set; }
        [NotMapped] public string? Industry { get; set; }
        [NotMapped] public string? Location { get; set; }

        // ✅ EF Navigation Properties (using correct class names)
        public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
        public virtual CV? Cv { get; set; }  // ✅ Changed from Cv to CV
        public virtual ICollection<Internship> Internships { get; set; } = new List<Internship>();
        public virtual ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();  // ✅ Changed to PaymentTransaction
    }
}