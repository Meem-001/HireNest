using System;
using System.Collections.Generic;
using System.Linq;
using InternshipManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagementSystem.Data
{
    public class ApplicationRepository
    {
        private readonly AppDbContext _context;

        public ApplicationRepository(AppDbContext context)
        {
            _context = context;
        }

        public bool HasAlreadyApplied(int studentId, int internshipId)
        {
            return _context.Applications.Any(a => a.StudentId == studentId && a.InternshipId == internshipId);
        }

        public void CreateApplication(Application application)
        {
            _context.Applications.Add(application);
            _context.SaveChanges();
        }

        public List<Application> GetApplicationsForCompany(int companyId)
        {
            return _context.Applications
                .Include(a => a.Student)
                .Include(a => a.Internship)
                .Where(a => a.Internship.CompanyId == companyId)
                .Select(a => new Application
                {
                    Id = a.Id,
                    StudentId = a.StudentId,
                    InternshipId = a.InternshipId,
                    Status = a.Status,
                    AppliedAt = a.AppliedAt,
                    ResumeFileName = a.ResumeFileName,
                    Feedback = a.Feedback,
                    StudentName = a.Student != null ? a.Student.FullName : "Unknown",
                    StudentEmail = a.Student != null ? a.Student.Email : "",
                    InternshipTitle = a.Internship != null ? a.Internship.Title : "Unknown"
                })
                .OrderByDescending(a => a.AppliedAt)
                .ToList();
        }

        public void UpdateApplicationStatus(int applicationId, string status, string feedback = null)
        {
            var app = _context.Applications.Find(applicationId);
            if (app != null)
            {
                app.Status = status;
                app.Feedback = feedback;
                _context.SaveChanges();
            }
        }

        public List<Application> GetAllApplications()
        {
            return _context.Applications
                .Include(a => a.Student)
                .Include(a => a.Internship).ThenInclude(i => i.Company)
                .Select(a => new Application
                {
                    Id = a.Id,
                    StudentId = a.StudentId,
                    InternshipId = a.InternshipId,
                    Status = a.Status,
                    AppliedAt = a.AppliedAt,
                    ResumeFileName = a.ResumeFileName,
                    Feedback = a.Feedback,
                    StudentName = a.Student != null ? a.Student.FullName : "Unknown",
                    StudentEmail = a.Student != null ? a.Student.Email : "",
                    InternshipTitle = a.Internship != null ? a.Internship.Title : "Unknown",
                    CompanyName = a.Internship != null && a.Internship.Company != null ? a.Internship.Company.FullName : "Unknown"
                })
                .OrderByDescending(a => a.AppliedAt)
                .ToList();
        }

        public List<Application> GetApplicationsByStudent(int studentId)
        {
            return _context.Applications
                .Include(a => a.Internship)
                .ThenInclude(i => i.Company)  // ✅ Include the Company
                .Where(a => a.StudentId == studentId)
                .Select(a => new Application
                {
                    Id = a.Id,
                    StudentId = a.StudentId,
                    InternshipId = a.InternshipId,
                    Status = a.Status,
                    AppliedAt = a.AppliedAt,
                    ResumeFileName = a.ResumeFileName,
                    Feedback = a.Feedback,
                    InternshipTitle = a.Internship != null ? a.Internship.Title : "Unknown",
                    InternshipLocation = a.Internship != null ? a.Internship.Location : "Unknown",
                    CompanyName = a.Internship != null && a.Internship.Company != null ? a.Internship.Company.FullName : "Unknown"  // ✅ Get Company Name
                })
                .OrderByDescending(a => a.AppliedAt)
                .ToList();
        }

        public void DeleteApplication(int id)
        {
            var app = _context.Applications.Find(id);
            if (app != null) _context.Applications.Remove(app);
            _context.SaveChanges();
        }

        public Application GetApplicationById(int id)
        {
            return _context.Applications
                .Include(a => a.Internship)
                .Where(a => a.Id == id)
                .Select(a => new Application
                {
                    Id = a.Id,
                    StudentId = a.StudentId,
                    InternshipId = a.InternshipId,
                    Status = a.Status,
                    AppliedAt = a.AppliedAt,
                    ResumeFileName = a.ResumeFileName,
                    Feedback = a.Feedback,
                    InternshipTitle = a.Internship != null ? a.Internship.Title : "Unknown"
                })
                .FirstOrDefault();
        }

        public User GetStudentById(int studentId)
        {
            return _context.Users
                .Where(u => u.Id == studentId)
                .Select(u => new User { Id = u.Id, FullName = u.FullName, Email = u.Email })
                .FirstOrDefault();
        }

        public Dictionary<string, int> GetApplicationStatusByCompany(int companyId)
        {
            return _context.Applications
                .Where(a => a.Internship.CompanyId == companyId)
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionary(x => x.Status, x => x.Count);
        }
    }
}