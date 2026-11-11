using System;
using System.Collections.Generic;
using System.Linq;
using InternshipManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagementSystem.Data
{
    public class UserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public bool EmailExists(string email)
        {
            return _context.Users.Any(u => u.Email == email);
        }

        public int CreateUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();

            // ✅ If it's a Company, create CompanyProfile
            if (user.Role == "Company")
            {
                var profile = new CompanyProfile // ⚠️ Change to 'Companyprofile' if your class name has lowercase 'p'
                {
                    CompanyId = user.Id,
                    PostsRemaining = 5,
                    PackageType = "Free"
                };
                _context.CompanyProfiles.Add(profile);
                _context.SaveChanges();
            }

            return user.Id;
        }

        public User GetUserByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }

        public List<User> GetAllUsers()
        {
            return _context.Users.OrderByDescending(u => u.CreatedAt).ToList();
        }

        public List<User> GetUsersByRole(string role)
        {
            return _context.Users.Where(u => u.Role == role).OrderByDescending(u => u.CreatedAt).ToList();
        }

        public void DeleteUser(int id)
        {
            var profile = _context.CompanyProfiles.FirstOrDefault(cp => cp.CompanyId == id);
            if (profile != null) _context.CompanyProfiles.Remove(profile);

            var user = _context.Users.Find(id);
            if (user != null) _context.Users.Remove(user);

            _context.SaveChanges();
        }

        public User GetUserById(int id)
        {
            return _context.Users.Find(id);
        }

        public void UpdateUser(User user)
        {
            var existing = _context.Users.Find(user.Id);
            if (existing != null)
            {
                existing.FullName = user.FullName;
                existing.Skills = user.Skills;
                _context.SaveChanges();
            }
        }
    }
}