using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternshipManagementSystem.Models
{
    public class Application
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int InternshipId { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime AppliedAt { get; set; } = DateTime.Now;
        public string? ResumeFileName { get; set; }
        public string? Feedback { get; set; }

        // 🚫 Tell EF these are from JOINs, not the 'applications' table
        [NotMapped] public string? InternshipTitle { get; set; }
        [NotMapped] public string? InternshipLocation { get; set; }
        [NotMapped] public string? StudentName { get; set; }
        [NotMapped] public string? StudentEmail { get; set; }
        [NotMapped] public string? CompanyName { get; set; }

        // ✅ EF Navigation Properties
        public virtual User? Student { get; set; }
        public virtual Internship? Internship { get; set; }
    }
}