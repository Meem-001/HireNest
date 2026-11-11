using System.Linq;
using InternshipManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagementSystem.Data
{
    public class CVRepository
    {
        private readonly AppDbContext _context;

        public CVRepository(AppDbContext context)
        {
            _context = context;
        }

        public CV GetByStudentId(int studentId)
        {
            return _context.Cvs.FirstOrDefault(cv => cv.StudentId == studentId);
        }

        public void SaveCV(CV cv)
        {
            var existing = _context.Cvs.FirstOrDefault(c => c.StudentId == cv.StudentId);

            if (existing == null)
            {
                _context.Cvs.Add(cv);
            }
            else
            {
                existing.Phone = cv.Phone;
                existing.LinkedIn = cv.LinkedIn;
                existing.Skills = cv.Skills;
                existing.Interests = cv.Interests;
                existing.Objective = cv.Objective;
                existing.Education = cv.Education;
                existing.Experience = cv.Experience;
                existing.PhotoFileName = cv.PhotoFileName;
            }

            _context.SaveChanges();
        }
    }
}