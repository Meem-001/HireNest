using Dapper;
using InternshipManagementSystem.Models;

namespace InternshipManagementSystem.Data
{
    public class TransactionRepository
    {
        private readonly DbHelper _dbHelper;

        public TransactionRepository(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public int CreateTransaction(PaymentTransaction transaction)  // ✅ Return int instead of void
        {
            using (var connection = _dbHelper.GetConnection())
            {
                var sql = @"INSERT INTO Transactions (CompanyId, PackageName, Amount, TransactionId, Status, CreatedAt) 
                   VALUES (@CompanyId, @PackageName, @Amount, @TransactionId, @Status, @CreatedAt);
                   SELECT LAST_INSERT_ID();";
                return connection.ExecuteScalar<int>(sql, transaction);  // ✅ Returns the new ID
            }
        }

        // ✅ NEW: Get all transactions for Admin
        public List<PaymentTransaction> GetAllTransactions()
        {
            using (var connection = _dbHelper.GetConnection())
            {
                return connection.Query<PaymentTransaction>("SELECT * FROM Transactions ORDER BY CreatedAt DESC").ToList();
            }
        }

        // ✅ NEW: Get only Pending transactions for Admin Approval
        public List<PaymentTransaction> GetPendingTransactions()
        {
            using (var connection = _dbHelper.GetConnection())
            {
                return connection.Query<PaymentTransaction>(
                    "SELECT * FROM Transactions WHERE Status = 'Pending' ORDER BY CreatedAt DESC"
                ).ToList();
            }
        }

        // ✅ NEW: Get transactions for a specific Company (for Receipts)
        public List<PaymentTransaction> GetTransactionsByCompanyId(int companyId)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                return connection.Query<PaymentTransaction>(
                    "SELECT * FROM Transactions WHERE CompanyId = @CompanyId ORDER BY CreatedAt DESC",
                    new { CompanyId = companyId }
                ).ToList();
            }
        }

        // ✅ FIXED: Approve and add +50 posts to CompanyProfile (not Users)
        public void ApproveTransaction(int transactionId, int postsToAdd)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                var transaction = connection.QueryFirstOrDefault<PaymentTransaction>(
                    "SELECT * FROM Transactions WHERE Id = @Id", new { Id = transactionId });

                if (transaction != null && transaction.Status == "Pending")
                {
                    // 1. Update Transaction Status
                    connection.Execute(
                        "UPDATE Transactions SET Status = 'Approved' WHERE Id = @Id",
                        new { Id = transactionId });

                    // 2. ✅ FIXED: Update CompanyProfiles table instead of Users
                    connection.Execute(
                        "UPDATE CompanyProfiles SET PostsRemaining = PostsRemaining + @PostsToAdd WHERE CompanyId = @CompanyId",
                        new { PostsToAdd = postsToAdd, CompanyId = transaction.CompanyId });
                }
            }
        }

        // ✅ NEW: Reject a transaction
        public void RejectTransaction(int transactionId)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Execute(
                    "UPDATE Transactions SET Status = 'Rejected' WHERE Id = @Id",
                    new { Id = transactionId });
            }
        }
    }
}