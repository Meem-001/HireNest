using InternshipManagementSystem.Data;
using InternshipManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InternshipManagementSystem.Controllers
{
    [Authorize(Roles = "Company")]
    public class PaymentController : Controller
    {
        private readonly TransactionRepository _transRepo;

        public PaymentController(TransactionRepository transRepo)
        {
            _transRepo = transRepo;
        }

        public IActionResult Checkout(string package = "Premium")
        {
            ViewBag.Package = package;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(string package, decimal amount, string cardNumber, string cardExpiry, string cardCvv)
        {
            var companyId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var transaction = new PaymentTransaction
            {
                CompanyId = companyId,
                PackageName = package,
                Amount = amount,
                TransactionId = "TXN-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                Status = "Pending",
                CreatedAt = DateTime.Now
            };
            transaction.Id = _transRepo.CreateTransaction(transaction);  // ✅ Capture the new ID
           

            // ✅ Redirect to the Receipt page instead of Home
            return RedirectToAction("Receipt", new { id = transaction.Id });
        }

        // ✅ NEW: Show the Receipt to the Company
        public IActionResult Receipt(int id)
        {
            var companyId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var transaction = _transRepo.GetAllTransactions().FirstOrDefault(t => t.Id == id && t.CompanyId == companyId);

            if (transaction == null) return NotFound();

            return View(transaction);
        }

        // ✅ NEW: View Payment History
        public IActionResult History()
        {
            var companyId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var transactions = _transRepo.GetTransactionsByCompanyId(companyId);
            return View(transactions);
        }
    }
}