using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternshipManagementSystem.Models
{
    public class CV
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string? Phone { get; set; }
        public string? LinkedIn { get; set; }
        public string? Skills { get; set; }
        public string? Interests { get; set; }
        public string? Objective { get; set; }
        public string? Education { get; set; }
        public string? Experience { get; set; }
        public string? PhotoFileName { get; set; }

        // 🚫 Tell EF these are NOT in the 'cvs' table (they come from the Users table)
        [NotMapped]
        public string? StudentName { get; set; }

        [NotMapped]
        public string? StudentEmail { get; set; }

        // ✅ Navigation property to link to the User
        public virtual User? Student { get; set; }
    }
}