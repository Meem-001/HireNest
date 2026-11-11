using InternshipManagementSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<HomeController> _logger;

        // ? Inject AppDbContext instead of DbHelper
        public HomeController(AppDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Pass login success message to view
            ViewBag.LoginMessage = TempData["LoginSuccess"] as string;

            string message = "Database Not Connected";
            try
            {
                // ? EF Core way to check database connection
                if (_context.Database.CanConnect())
                {
                    message = "? Successfully Connected to MySQL via Entity Framework Core!";
                }
            }
            catch (Exception ex)
            {
                message = "? Connection Failed: " + ex.Message;
            }

            ViewBag.ConnectionStatus = message;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}