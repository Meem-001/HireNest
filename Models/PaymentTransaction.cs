namespace InternshipManagementSystem.Models
{
    public class PaymentTransaction
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}