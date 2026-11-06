using InternshipManagementSystem.Data;
using InternshipManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using System.IO;

namespace InternshipManagementSystem.Controllers
{
    [Authorize(Roles = "Student")]
    public class CVController : Controller
    {
        private readonly CVRepository _cvRepo;
        private readonly UserRepository _userRepository;

        public CVController(CVRepository cvRepo, UserRepository userRepository)
        {
            _cvRepo = cvRepo;
            _userRepository = userRepository;
        }

        // GET: Build/Edit CV
        public IActionResult Edit()
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var cv = _cvRepo.GetByStudentId(studentId);

            if (cv == null)
            {
                cv = new CV { StudentId = studentId };
            }

            return View(cv);
        }

        // POST: Preview CV (Does NOT save to Database yet)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Preview(CV cv, IFormFile? photo)
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            cv.StudentId = studentId;

            // Get student name and email for preview
            var user = _userRepository.GetUserById(studentId);
            cv.StudentName = user?.FullName;
            cv.StudentEmail = user?.Email;

            // ✅ FIX: Handle Photo Upload OR keep existing photo
            if (photo != null && photo.Length > 0)
            {
                var ext = Path.GetExtension(photo.FileName).ToLowerInvariant();
                if (ext == ".jpg" || ext == ".jpeg" || ext == ".png")
                {
                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "photos", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await photo.CopyToAsync(stream);
                    cv.PhotoFileName = fileName;
                }
            }
            else
            {
                // 🛡️ PROTECTION: If no new photo was uploaded, keep the existing photo from the database
                var existingCV = _cvRepo.GetByStudentId(studentId);
                if (existingCV != null && !string.IsNullOrEmpty(existingCV.PhotoFileName))
                {
                    cv.PhotoFileName = existingCV.PhotoFileName;
                }
            }

            // Store CV data in TempData for the preview page
            TempData["PendingCV"] = JsonSerializer.Serialize(cv);
            TempData.Keep("PendingCV"); // Keep it so the View action can read it

            return RedirectToAction("View", new { preview = true });
        }

        // POST: Confirm Save (Actually saves to Database)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmSave()
        {
            if (TempData["PendingCV"] != null)
            {
                var cv = JsonSerializer.Deserialize<CV>(TempData["PendingCV"].ToString());
                if (cv != null)
                {
                    _cvRepo.SaveCV(cv);
                    TempData["Success"] = "CV saved successfully!";
                }
            }
            return RedirectToAction("View"); // Go to final view without preview flag
        }

        // GET: View CV (Handles both Preview mode and Final Saved mode)
        public IActionResult View(bool preview = false)
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            CV cv = null;
            bool isPreview = false;

            if (preview && TempData["PendingCV"] != null)
            {
                // Load from TempData for preview
                cv = JsonSerializer.Deserialize<CV>(TempData["PendingCV"].ToString());
                isPreview = true;
                TempData.Keep("PendingCV"); // Keep it for the ConfirmSave action
            }
            else
            {
                // Load from Database for final view
                cv = _cvRepo.GetByStudentId(studentId);
                if (cv == null)
                {
                    TempData["Warning"] = "You haven't built your CV yet.";
                    return RedirectToAction("Edit");
                }
            }

            ViewBag.IsPreview = isPreview;
            return View(cv);
        }
    }
}