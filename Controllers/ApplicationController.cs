using InternshipManagementSystem.Data;
using InternshipManagementSystem.Models;
using InternshipManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace InternshipManagementSystem.Controllers
{
    [Authorize]
    public class ApplicationController : Controller
    {
        private readonly ApplicationRepository _appRepo;
        private readonly InternshipRepository _intRepo;
        private readonly EmailService _emailService;
        private readonly CVRepository _cvRepo;  // ✅ CVRepository (capital CV)

        public ApplicationController(ApplicationRepository appRepo, InternshipRepository intRepo, EmailService emailService, CVRepository cvRepo)  // ✅ CVRepository
        {
            _appRepo = appRepo;
            _intRepo = intRepo;
            _emailService = emailService;
            _cvRepo = cvRepo;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(int internshipId, IFormFile? resume)
        {
            try
            {
                var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                // ✅ Check if student has built their CV first
                var cv = _cvRepo.GetByStudentId(studentId);  // ✅ GetByStudentId (not GetCvByUserId)
                if (cv == null)
                {
                    TempData["Error"] = "You must build your CV first before applying to internships. Please go to 'My CV' and complete your profile.";
                    return RedirectToAction("Details", "Internship", new { id = internshipId });
                }

                if (_appRepo.HasAlreadyApplied(studentId, internshipId))
                {
                    TempData["Error"] = "You have already applied to this internship.";
                    return RedirectToAction("Details", "Internship", new { id = internshipId });
                }

                string? fileName = null;

                // 1. Check if file was actually received
                if (resume != null && resume.Length > 0)
                {
                    System.Console.WriteLine($"File Received: {resume.FileName}, Size: {resume.Length} bytes");

                    var ext = Path.GetExtension(resume.FileName).ToLowerInvariant();

                    if (ext != ".pdf" && ext != ".doc" && ext != ".docx")
                    {
                        TempData["Error"] = $"Invalid file type: {ext}. Only PDF, DOC, DOCX allowed.";
                        return RedirectToAction("Details", "Internship", new { id = internshipId });
                    }

                    // 2MB Limit (Safer than 5MB to avoid server limits)
                    if (resume.Length > 2 * 1024 * 1024)
                    {
                        TempData["Error"] = "File too large. Max 2MB.";
                        return RedirectToAction("Details", "Internship", new { id = internshipId });
                    }

                    // 2. Create Directory if it doesn't exist (Prevents "Path not found" errors)
                    string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "resumes");
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                        System.Console.WriteLine("Created uploads/resumes folder!");
                    }

                    // 3. Save File
                    fileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await resume.CopyToAsync(stream);
                    }

                    System.Console.WriteLine($"Saved file to: {filePath}");
                }
                else
                {
                    System.Console.WriteLine("No file received, or file was empty.");
                }

                // 4. Save to Database
                var application = new Application
                {
                    StudentId = studentId,
                    InternshipId = internshipId,
                    Status = "Pending",
                    AppliedAt = DateTime.Now,
                    ResumeFileName = fileName // Will be null if no file
                };

                _appRepo.CreateApplication(application);
                TempData["Success"] = "Application submitted successfully!";
                return RedirectToAction("MyApplications");
            }
            catch (Exception ex)
            {
                // Show the EXACT error on the screen so we know what broke
                TempData["Error"] = "SYSTEM ERROR: " + ex.Message;
                System.Console.WriteLine("❌ UPLOAD ERROR: " + ex.Message);
                System.Console.WriteLine("❌ Stack Trace: " + ex.StackTrace);

                return RedirectToAction("Details", "Internship", new { id = internshipId });
            }
        }

        // Student: View my applications
        public IActionResult MyApplications()
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var applications = _appRepo.GetApplicationsByStudent(studentId);
            return View(applications);
        }

        // Company: View applications for their internships
        [Authorize(Roles = "Company")]
        public IActionResult CompanyApplications()
        {
            var companyId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var applications = _appRepo.GetApplicationsForCompany(companyId);

            // 📊 Chart Data
            var statusCounts = _appRepo.GetApplicationStatusByCompany(companyId);
            ViewBag.ChartLabels = JsonSerializer.Serialize(statusCounts.Keys);
            ViewBag.ChartData = JsonSerializer.Serialize(statusCounts.Values);

            return View(applications);
        }

        // Company: Accept/Reject application + Send Email
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Company")]

        public IActionResult UpdateStatus(int applicationId, string status, string feedback)
        {
            // 1. Update status AND feedback in DB
            _appRepo.UpdateApplicationStatus(applicationId, status, feedback);

            // 2. 📧 Send Email Notification with Feedback
            try
            {
                var app = _appRepo.GetApplicationById(applicationId);
                if (app != null)
                {
                    var student = _appRepo.GetStudentById(app.StudentId);

                    if (student != null && !string.IsNullOrEmpty(student.Email))
                    {
                        string subject = $"Internship Application: {status}";

                        // Add feedback to the email body if it exists
                        string feedbackHtml = "";
                        if (!string.IsNullOrWhiteSpace(feedback))
                        {
                            feedbackHtml = $@"
                        <div style='background-color: #f8f9fa; padding: 15px; border-left: 4px solid #0d6efd; margin: 20px 0;'>
                            <strong>Company Feedback:</strong><br/>
                            <em>{System.Net.WebUtility.HtmlEncode(feedback)}</em>
                        </div>";
                        }

                        string body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2 style='color: #333;'>Application Update</h2>
                        <p>Dear <strong>{student.FullName}</strong>,</p>
                        <p>Your application for the position of <strong>'{@app.InternshipTitle}'</strong> has been <strong style='color: {(status == "Accepted" ? "green" : "red")};'>{status}</strong>.</p>
                        {feedbackHtml}
                        <p>Thank you for applying!</p>
                        <hr/>
                        <small style='color: #666;'>This is an automated message from Internship Management System.</small>
                    </body>
                    </html>";

                        _emailService.SendEmail(student.Email, subject, body);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Email sending failed: " + ex.Message);
            }

            TempData["Success"] = $"Application {status.ToLower()} successfully!";
            return RedirectToAction("CompanyApplications");
        }
    }
}