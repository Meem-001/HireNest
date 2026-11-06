using InternshipManagementSystem.Data;
using InternshipManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InternshipManagementSystem.Controllers
{
    public class InternshipController : Controller
    {
        private readonly InternshipRepository _internshipRepo;
        private readonly UserRepository _userRepository;
        private readonly CompanyProfileRepository _companyProfileRepo;

        public InternshipController(
            InternshipRepository internshipRepo,
            UserRepository userRepository,
            CompanyProfileRepository companyProfileRepo)
        {
            _internshipRepo = internshipRepo;
            _userRepository = userRepository;
            _companyProfileRepo = companyProfileRepo;
        }

        // ✅ PUBLIC: View all internships (No login required)
        public IActionResult Index(string searchString, string filterLocation, string filterDuration, string sortBy, int pageNumber = 1)
        {
            var internships = _internshipRepo.GetAllInternships();
            var studentSkills = new List<string>();
            var matchScores = new Dictionary<int, int>();

            if (User.Identity.IsAuthenticated && User.IsInRole("Student"))
            {
                var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var user = _userRepository.GetUserById(studentId);
                if (!string.IsNullOrEmpty(user?.Skills))
                {
                    studentSkills = user.Skills.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                               .Select(s => s.Trim().ToLower()).ToList();
                }
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                internships = internships.Where(i =>
                    i.Title.ToLower().Contains(searchString.ToLower()) ||
                    i.Description.ToLower().Contains(searchString.ToLower()) ||
                    i.RequiredSkills.ToLower().Contains(searchString.ToLower()) ||
                    i.Location.ToLower().Contains(searchString.ToLower())
                ).ToList();
            }
            if (!string.IsNullOrEmpty(filterLocation))
                internships = internships.Where(i => i.Location == filterLocation).ToList();
            if (!string.IsNullOrEmpty(filterDuration))
                internships = internships.Where(i => i.Duration == filterDuration).ToList();

            IEnumerable<Internship> sortedInternships;

            switch (sortBy)
            {
                case "Title":
                    sortedInternships = internships.OrderBy(i => i.Title);
                    break;
                case "Duration":
                    sortedInternships = internships.OrderBy(i => ConvertDurationToMonths(i.Duration));
                    break;
                case "Oldest":
                    sortedInternships = internships.OrderBy(i => i.CreatedAt);
                    break;
                default:
                    sortedInternships = internships.OrderByDescending(i => i.CreatedAt);
                    break;
            }

            int pageSize = 4;
            var totalItems = sortedInternships.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.TotalItems = totalItems; // ✅ NEW

            if (pageNumber < 1) pageNumber = 1;
            if (pageNumber > totalPages) pageNumber = totalPages > 0 ? totalPages : 1;

            var paginatedInternships = sortedInternships.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            foreach (var internship in paginatedInternships)
            {
                int score = CalculateMatchScore(internship.RequiredSkills, studentSkills);
                matchScores[internship.Id] = score;
            }

            ViewBag.MatchScores = matchScores;
            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentSort = sortBy;
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentLocation = filterLocation;
            ViewBag.CurrentDuration = filterDuration;
            ViewBag.Locations = internships.Select(i => i.Location).Distinct().OrderBy(x => x).ToList();
            ViewBag.Durations = internships.Select(i => i.Duration).Distinct().OrderBy(x => x).ToList();

            return View(paginatedInternships);
        }

        // 🔒 COMPANY ONLY: View their own internships (with auto-create profile fix)
        [Authorize(Roles = "Company")]
        public IActionResult MyInternships()
        {
            var companyId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var internships = _internshipRepo.GetInternshipsByCompanyId(companyId);

            var profile = _companyProfileRepo.GetByCompanyId(companyId);

            // If profile doesn't exist, create it automatically
            if (profile == null)
            {
                profile = new CompanyProfile
                {
                    CompanyId = companyId,
                    PostsRemaining = 5,
                    PackageType = "Free"
                };
                _companyProfileRepo.CreateCompanyProfile(profile);

                // Re-fetch to confirm
                profile = _companyProfileRepo.GetByCompanyId(companyId);
            }

            ViewBag.PostsRemaining = profile?.PostsRemaining ?? 2;
            ViewBag.PackageType = profile?.PackageType ?? "Free";

            return View(internships);
        }

        // 🔒 COMPANY ONLY: Create Internship Form
        [Authorize(Roles = "Company")]
        public IActionResult Create()
        {
            var companyId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var profile = _companyProfileRepo.GetByCompanyId(companyId);

            // Auto-create profile if missing
            if (profile == null)
            {
                profile = new CompanyProfile
                {
                    CompanyId = companyId,
                    PostsRemaining = 5,
                    PackageType = "Free"
                };
                _companyProfileRepo.CreateCompanyProfile(profile);
            }

            if (profile.PostsRemaining <= 0)
            {
                TempData["Warning"] = "You have no posts remaining. Please upgrade your package.";
                return RedirectToAction("MyInternships");
            }

            ViewBag.PostsRemaining = profile.PostsRemaining;
            return View();
        }

        // 🔒 COMPANY ONLY: Save Internship
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Company")]
        public IActionResult Create(Internship internship)
        {
            var companyId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var companyProfile = _companyProfileRepo.GetByCompanyId(companyId);

            if (!ModelState.IsValid)
            {
                ViewBag.PostsRemaining = companyProfile?.PostsRemaining ?? 0;
                return View(internship);
            }

            if (companyProfile == null || companyProfile.PostsRemaining <= 0)
            {
                TempData["Error"] = "No posts remaining! Please upgrade your package.";
                return RedirectToAction("Checkout", "Payment");
            }

            internship.CompanyId = companyId;
            internship.CreatedAt = DateTime.Now;
            _internshipRepo.CreateInternship(internship);

            // Decrement posts remaining
            var newPostsRemaining = companyProfile.PostsRemaining - 1;
            _companyProfileRepo.UpdatePostsRemaining(companyId, newPostsRemaining);

            TempData["Success"] = "Internship posted successfully!";
            TempData["PostsRemaining"] = newPostsRemaining;

            return RedirectToAction("MyInternships");
        }

        // 🔒 COMPANY ONLY: Edit Internship (GET) - THIS WAS MISSING!
        [Authorize(Roles = "Company")]
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var internship = _internshipRepo.GetInternshipById(id.Value);
            if (internship == null) return NotFound();

            var companyId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (internship.CompanyId != companyId) return Forbid();

            return View(internship);
        }

        // 🔒 COMPANY ONLY: Update Internship (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Company")]
        public IActionResult Edit(Internship internship)
        {
            if (!ModelState.IsValid) return View(internship);

            var companyId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var existing = _internshipRepo.GetInternshipById(internship.Id);
            if (existing == null || existing.CompanyId != companyId) return Forbid();

            _internshipRepo.UpdateInternship(internship);
            TempData["Success"] = "Internship updated successfully!";
            return RedirectToAction("MyInternships");
        }

        // 🔒 COMPANY ONLY: Delete Internship
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Company")]
        public IActionResult Delete(int id)
        {
            var companyId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var internship = _internshipRepo.GetInternshipById(id);
            if (internship == null || internship.CompanyId != companyId) return Forbid();

            _internshipRepo.DeleteInternship(id);
            TempData["Success"] = "Internship deleted successfully!";
            return RedirectToAction("MyInternships");
        }

        // ✅ PUBLIC: Internship Details Page
        public IActionResult Details(int id)
        {
            var internship = _internshipRepo.GetInternshipWithCompany(id);
            if (internship == null) return NotFound();

            int matchScore = 0;
            if (User.Identity.IsAuthenticated && User.IsInRole("Student"))
            {
                var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var student = _userRepository.GetUserById(studentId);
                if (!string.IsNullOrEmpty(student?.Skills))
                {
                    matchScore = CalculateMatchScore(internship.RequiredSkills,
                        student.Skills.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(s => s.Trim().ToLower()).ToList());
                }
            }

            ViewBag.MatchScore = matchScore;
            return View(internship);
        }

        private int CalculateMatchScore(string requiredSkillsStr, List<string> studentSkills)
        {
            if (!studentSkills.Any()) return 0;

            var required = requiredSkillsStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                            .Select(s => s.Trim().ToLower()).ToList();
            if (!required.Any()) return 0;

            int matched = required.Count(r => studentSkills.Contains(r));
            return (matched * 100) / required.Count;
        }

        private int ConvertDurationToMonths(string duration)
        {
            if (string.IsNullOrEmpty(duration)) return 0;

            var text = duration.ToLower();
            var match = System.Text.RegularExpressions.Regex.Match(text, @"\d+");
            if (!match.Success) return 0;

            int number = int.Parse(match.Value);

            if (text.Contains("year"))
                return number * 12;

            return number;
        }
    }
}