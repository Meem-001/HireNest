using InternshipManagementSystem.Data;
using InternshipManagementSystem.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InternshipManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserRepository _userRepository;
        private readonly CompanyProfileRepository _companyProfileRepo; // ✅ NEW

        public AccountController(UserRepository userRepository, CompanyProfileRepository companyProfileRepo) // ✅ NEW
        {
            _userRepository = userRepository;
            _companyProfileRepo = companyProfileRepo; // ✅ NEW
        }

        // GET: Registration Page
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (!ModelState.IsValid) return View(user);

            if (_userRepository.GetUserByEmail(user.Email) != null)
            {
                ModelState.AddModelError("Email", "Email already registered.");
                return View(user);
            }

            user.Role = "Student";
            user.PostsRemaining = 0;
            user.CreatedAt = DateTime.Now;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            // ✅ FIX: Capture the new ID so login claims are correct
            user.Id = _userRepository.CreateUser(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity)).Wait();

            return RedirectToAction("Index", "Home");
        }

        // GET: Company Register
        [HttpGet]
        public IActionResult RegisterCompany()
        {
            return View();
        }

        // POST: Company Register
        [HttpPost]
        public IActionResult RegisterCompany(User user, string industry = null, string companySize = null, string website = null)
        {
            if (!ModelState.IsValid) return View(user);

            if (_userRepository.GetUserByEmail(user.Email) != null)
            {
                ModelState.AddModelError("Email", "Email already registered.");
                return View(user);
            }

            user.Role = "Company";
            user.PostsRemaining = 5;
            user.CreatedAt = DateTime.Now;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            // ✅ FIX: Capture the new ID so login claims are correct
            user.Id = _userRepository.CreateUser(user);

            // ✅ Save Industry & Website into the CompanyProfile
            var profile = _companyProfileRepo.GetByCompanyId(user.Id);
            if (profile != null)
            {
                profile.Industry = industry;
                profile.Website = website;
                // Note: companySize is collected for UX (not stored yet)
                _companyProfileRepo.UpdateCompanyProfile(profile);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity)).Wait();

            return RedirectToAction("MyInternships", "Internship");
        }

        // GET: Login Page (Accepts role from URL)
        [HttpGet]
        public IActionResult Login(string role = "Student")
        {
            ViewBag.Role = role;
            return View();
        }

        // POST: Handle Login with Role Check
        [HttpPost]
        public IActionResult Login(string email, string password, string role)
        {
            var user = _userRepository.GetUserByEmail(email);

            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid email or password.");
                ViewBag.Role = role;
                return View();
            }

            if (user.Role != role)
            {
                ModelState.AddModelError("", $"This account is registered as a {user.Role}. Please use the correct login page.");
                ViewBag.Role = role;
                return View();
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid email or password.");
                ViewBag.Role = role;
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = true };

            HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties).Wait();

            if (user.Role == "Admin")
                return RedirectToAction("Index", "Admin");
            else if (user.Role == "Company")
                return RedirectToAction("MyInternships", "Internship");
            else
                return RedirectToAction("Index", "Internship");
        }

        // GET: Admin Login
        [HttpGet]
        public IActionResult AdminLogin()
        {
            return View();
        }

        // POST: Admin Login
        [HttpPost]
        public IActionResult AdminLogin(string email, string password)
        {
            var user = _userRepository.GetUserByEmail(email);

            if (user == null || user.Role != "Admin" || string.IsNullOrEmpty(user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid admin credentials.");
                return View();
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid admin credentials.");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity)).Wait();

            return RedirectToAction("Index", "Admin");
        }

        // GET: Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // GET: Edit Profile
        [Authorize]
        public IActionResult EditProfile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = _userRepository.GetUserById(userId);
            return View(user);
        }

        // POST: Save Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult EditProfile(User user)
        {
            if (!ModelState.IsValid) return View(user);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            user.Id = userId;

            _userRepository.UpdateUser(user);
            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("EditProfile");
        }
    }
}