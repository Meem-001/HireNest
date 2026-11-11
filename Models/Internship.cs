using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternshipManagementSystem.Models
{
    public class Internship
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RequiredSkills { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string? Salary { get; set; } = "৳25,000/mo";
        public string? JobType { get; set; } = "Hybrid";
        public DateTime? ClosingDate { get; set; }
        public int CompanyId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 🚫 Tell EF these are from JOINs, not the 'internships' table
        [NotMapped] public string? CompanyName { get; set; }
        [NotMapped] public int ApplicantCount { get; set; }

        // ✅ EF Navigation Properties
        public virtual User? Company { get; set; }
        public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
    }
}