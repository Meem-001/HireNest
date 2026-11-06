using InternshipManagementSystem.Data;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace InternshipManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly DbHelper _dbHelper;
        private readonly ILogger<HomeController> _logger;

        public HomeController(DbHelper dbHelper, ILogger<HomeController> logger)
        {
            _dbHelper = dbHelper;
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Pass login success message to view
            ViewBag.LoginMessage = TempData["LoginSuccess"] as string;

            string message = "Database Not Connected";
            try
            {
                using (var connection = _dbHelper.GetConnection())
                {
                    connection.Open();
                    message = "? Successfully Connected to MySQL!";
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