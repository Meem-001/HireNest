using System;
using System.Collections.Generic;
using System.Linq;
using InternshipManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagementSystem.Data
{
    public class InternshipRepository
    {
        private readonly AppDbContext _context;

        public InternshipRepository(AppDbContext context)
        {
            _context = context;
        }

        public int CreateInternship(Internship internship)
        {
            _context.Internships.Add(internship);
            _context.SaveChanges();
            return internship.Id;
        }

        public List<Internship> GetAllInternships()
        {
            return _context.Internships
                .Include(i => i.Company)
                .Include(i => i.Applications)
                .Select(i => new Internship
                {
                    Id = i.Id,
                    Title = i.Title,
                    Description = i.Description,
                    RequiredSkills = i.RequiredSkills,
                    Location = i.Location,
                    Duration = i.Duration,
                    Salary = i.Salary,
                    JobType = i.JobType,
                    ClosingDate = i.ClosingDate,
                    CompanyId = i.CompanyId,
                    CreatedAt = i.CreatedAt,
                    CompanyName = i.Company != null ? i.Company.FullName : "Unknown",
                    ApplicantCount = i.Applications.Count
                })
                .OrderByDescending(i => i.CreatedAt)
                .ToList();
        }

        public List<Internship> GetInternshipsByCompanyId(int companyId)
        {
            return _context.Internships
                .Where(i => i.CompanyId == companyId)
                .OrderByDescending(i => i.CreatedAt)
                .ToList();
        }

        public Internship GetInternshipById(int id)
        {
            return _context.Internships.Find(id);
        }

        public void UpdateInternship(Internship internship)
        {
            var existing = _context.Internships.Find(internship.Id);
            if (existing != null)
            {
                existing.Title = internship.Title;
                existing.Description = internship.Description;
                existing.RequiredSkills = internship.RequiredSkills;
                existing.Location = internship.Location;
                existing.Duration = internship.Duration;
                existing.Salary = internship.Salary;
                existing.JobType = internship.JobType;
                existing.ClosingDate = internship.ClosingDate;
                _context.SaveChanges();
            }
        }

        public void DeleteInternship(int id)
        {
            var apps = _context.Applications.Where(a => a.InternshipId == id).ToList();
            _context.Applications.RemoveRange(apps);

            var internship = _context.Internships.Find(id);
            if (internship != null) _context.Internships.Remove(internship);

            _context.SaveChanges();
        }

        public Dictionary<string, int> GetTopRequiredSkills()
        {
            var skills = _context.Internships.Select(i => i.RequiredSkills).ToList();
            var skillCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var s in skills)
            {
                var parts = s.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
                foreach (var part in parts)
                    skillCounts[part] = skillCounts.GetValueOrDefault(part, 0) + 1;
            }
            return skillCounts.OrderByDescending(x => x.Value).Take(5).ToDictionary(x => x.Key, x => x.Value);
        }

        public InternshipDetailsViewModel GetInternshipWithCompany(int id)
        {
            return _context.Internships
                .Include(i => i.Company)
                .Where(i => i.Id == id)
                .Select(i => new InternshipDetailsViewModel
                {
                    Id = i.Id,
                    Title = i.Title,
                    Description = i.Description,
                    RequiredSkills = i.RequiredSkills,
                    Location = i.Location,
                    Duration = i.Duration,
                    Salary = i.Salary,
                    JobType = i.JobType,
                    ClosingDate = i.ClosingDate,
                    CompanyId = i.CompanyId,
                    CreatedAt = i.CreatedAt,
                    CompanyName = i.Company != null ? i.Company.FullName : "Unknown",
                    CompanyEmail = i.Company != null ? i.Company.Email : ""
                })
                .FirstOrDefault();
        }
    }
}