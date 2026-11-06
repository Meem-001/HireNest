using InternshipManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InternshipManagementSystem.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly UserRepository _userRepository;

        public StudentController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // GET: Skill Match Settings
        public IActionResult SkillMatch()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = _userRepository.GetUserById(userId);
            return View(user);
        }

        // POST: Update Skills Only
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateSkills(string skills)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = _userRepository.GetUserById(userId);

            if (user != null)
            {
                user.Skills = skills;
                _userRepository.UpdateUser(user);
                TempData["Success"] = "Skills updated! Your match percentage has been recalculated.";
            }

            return RedirectToAction("SkillMatch");
        }
    }
}