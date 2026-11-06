using InternshipManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace InternshipManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserRepository _userRepo;
        private readonly InternshipRepository _internshipRepo;
        private readonly ApplicationRepository _appRepo;
        private readonly TransactionRepository _transRepo; // ✅ Exists

        // ✅ FIXED CONSTRUCTOR
        public AdminController(
            UserRepository userRepo,
            InternshipRepository internshipRepo,
            ApplicationRepository appRepo,
            TransactionRepository transRepo) // ✅ Added parameter
        {
            _userRepo = userRepo;
            _internshipRepo = internshipRepo;
            _appRepo = appRepo;
            _transRepo = transRepo; // ✅ Assigned here!
        }

        public IActionResult Index()
        {
            ViewBag.TotalUsers = _userRepo.GetAllUsers().Count;
            ViewBag.TotalInternships = _internshipRepo.GetAllInternships().Count;
            ViewBag.TotalApplications = _appRepo.GetAllApplications().Count;
            ViewBag.TotalStudents = _userRepo.GetUsersByRole("Student").Count;
            ViewBag.TotalCompanies = _userRepo.GetUsersByRole("Company").Count;
            ViewBag.PendingUpgrades = _transRepo.GetPendingTransactions().Count; // ✅ NEW

            // 📊 Chart Data
            var topSkills = _internshipRepo.GetTopRequiredSkills();
            ViewBag.SkillLabels = JsonSerializer.Serialize(topSkills.Keys);
            ViewBag.SkillData = JsonSerializer.Serialize(topSkills.Values);

            return View();
        }

        // View all users (excluding admins)
        public IActionResult Users()
        {
            var users = _userRepo.GetAllUsers()
                .Where(u => u.Role != "Admin")
                .ToList();
            return View(users);
        }

        // View all internships
        public IActionResult Internships()
        {
            var internships = _internshipRepo.GetAllInternships();
            return View(internships);
        }

        // View all applications
        public IActionResult Applications()
        {
            var applications = _appRepo.GetAllApplications();
            return View(applications);
        }

        // Delete user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(int id)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (id == currentUserId)
            {
                TempData["Error"] = "You cannot delete your own account!";
                return RedirectToAction("Users");
            }

            _userRepo.DeleteUser(id);
            TempData["Success"] = "User deleted successfully!";
            return RedirectToAction("Users");
        }

        // Admin: Delete Internship
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteInternship(int id)
        {
            _internshipRepo.DeleteInternship(id);
            TempData["Success"] = "Internship deleted by Admin.";
            return RedirectToAction("Internships");
        }

        // Admin: Update Application Status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateApplicationStatus(int id, string status)
        {
            _appRepo.UpdateApplicationStatus(id, status);
            TempData["Success"] = $"Application {status} by Admin.";
            return RedirectToAction("Applications");
        }

        // Admin: Delete Application
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteApplication(int id)
        {
            _appRepo.DeleteApplication(id);
            TempData["Success"] = "Application deleted.";
            return RedirectToAction("Applications");
        }

        // ✅ Admin: View Pending Upgrade Requests
        public IActionResult PendingUpgrades()
        {
            var pendingTransactions = _transRepo.GetPendingTransactions();
            return View(pendingTransactions);
        }

        // ✅ Admin: Approve Upgrade (+50 Posts)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveUpgrade(int id)
        {
            _transRepo.ApproveTransaction(id, 50);
            TempData["Success"] = "Upgrade approved! 50 posts have been added to the company.";
            return RedirectToAction("PendingUpgrades");
        }

        // ✅ Admin: Reject Upgrade
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectUpgrade(int id)
        {
            _transRepo.RejectTransaction(id);
            TempData["Warning"] = "Upgrade request rejected.";
            return RedirectToAction("PendingUpgrades");
        }
    }
}