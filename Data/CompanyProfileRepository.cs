using System.Linq;
using InternshipManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagementSystem.Data
{
    public class CompanyProfileRepository
    {
        private readonly AppDbContext _context;

        public CompanyProfileRepository(AppDbContext context)
        {
            _context = context;
        }

        public CompanyProfile GetByCompanyId(int companyId)
        {
            return _context.CompanyProfiles.FirstOrDefault(cp => cp.CompanyId == companyId);
        }

        public void CreateCompanyProfile(CompanyProfile profile)
        {
            _context.CompanyProfiles.Add(profile);
            _context.SaveChanges();
        }

        public void UpdatePostsRemaining(int companyId, int postsRemaining)
        {
            var profile = _context.CompanyProfiles.FirstOrDefault(cp => cp.CompanyId == companyId);
            if (profile != null)
            {
                profile.PostsRemaining = postsRemaining;
                _context.SaveChanges();
            }
        }

        public void UpdateCompanyProfile(CompanyProfile profile)
        {
            var existing = _context.CompanyProfiles.FirstOrDefault(cp => cp.CompanyId == profile.CompanyId);
            if (existing != null)
            {
                existing.Website = profile.Website;
                existing.CompanyDescription = profile.CompanyDescription;
                existing.Industry = profile.Industry;
                existing.Location = profile.Location;
                existing.PackageType = profile.PackageType;

                _context.SaveChanges();
            }
        }
    }
}