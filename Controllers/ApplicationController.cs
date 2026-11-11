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
        private readonly CVRepository _cvRepo;

        // ✅ THIS CONSTRUCTOR WAS MISSING! It injects the services so they aren't null.
        public ApplicationController(
            ApplicationRepository appRepo,
            InternshipRepository intRepo,
            EmailService emailService,
            CVRepository cvRepo)
        {
            _appRepo = appRepo ?? throw new ArgumentNullException(nameof(appRepo));
            _intRepo = intRepo ?? throw new ArgumentNullException(nameof(intRepo));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _cvRepo = cvRepo ?? throw new ArgumentNullException(nameof(cvRepo));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(int internshipId, IFormFile? resume)
        {
            try
            {
                var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                // ✅ 1. MANDATORY FILE CHECK: File MUST be uploaded
                if (resume == null || resume.Length == 0)
                {
                    TempData["Error"] = "You must attach a PDF resume file to apply for this internship.";
                    return RedirectToAction("Details", "Internship", new { id = internshipId });
                }

                // ✅ 2. STRICT PDF CHECK: Only PDF files allowed
                var ext = Path.GetExtension(resume.FileName).ToLowerInvariant();
                if (ext != ".pdf")
                {
                    TempData["Error"] = "Invalid file type. Only PDF files are allowed.";
                    return RedirectToAction("Details", "Internship", new { id = internshipId });
                }

                // ✅ 3. FILE SIZE CHECK (Max 2MB)
                if (resume.Length > 2 * 1024 * 1024)
                {
                    TempData["Error"] = "File too large. Max 2MB.";
                    return RedirectToAction("Details", "Internship", new { id = internshipId });
                }

                // ✅ 4. Check if already applied
                if (_appRepo.HasAlreadyApplied(studentId, internshipId))
                {
                    TempData["Error"] = "You have already applied to this internship.";
                    return RedirectToAction("Details", "Internship", new { id = internshipId });
                }

                // ✅ 5. Save the PDF File
                string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "resumes");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                string fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await resume.CopyToAsync(stream);
                }

                // ✅ 6. Create and save the application
                var application = new Application
                {
                    StudentId = studentId,
                    InternshipId = internshipId,
                    Status = "Pending",
                    AppliedAt = DateTime.Now,
                    ResumeFileName = fileName // Will always have a value now
                };

                _appRepo.CreateApplication(application);

                TempData["Success"] = "Application submitted successfully!";
                return RedirectToAction("MyApplications");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "SYSTEM ERROR: " + ex.Message;
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
            _appRepo.UpdateApplicationStatus(applicationId, status, feedback);

            try
            {
                var app = _appRepo.GetApplicationById(applicationId);
                if (app != null)
                {
                    var student = _appRepo.GetStudentById(app.StudentId);

                    if (student != null && !string.IsNullOrEmpty(student.Email))
                    {
                        string subject = $"Internship Application: {status}";

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
                        <p>Your application for the position of <strong>'{app.InternshipTitle}'</strong> has been <strong style='color: {(status == "Accepted" ? "green" : "red")};'>{status}</strong>.</p>
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