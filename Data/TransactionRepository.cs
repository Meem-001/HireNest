using System;
using System.Collections.Generic;
using System.Linq;
using InternshipManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagementSystem.Data
{
    public class TransactionRepository
    {
        private readonly AppDbContext _context;

        public TransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public int CreateTransaction(PaymentTransaction transaction)
        {
            _context.PaymentTransactions.Add(transaction);
            _context.SaveChanges();
            return transaction.Id; // EF automatically populates the Id after SaveChanges
        }

        public List<PaymentTransaction> GetAllTransactions()
        {
            return _context.PaymentTransactions
                .OrderByDescending(t => t.CreatedAt)
                .ToList();
        }

        public List<PaymentTransaction> GetPendingTransactions()
        {
            return _context.PaymentTransactions
                .Where(t => t.Status == "Pending")
                .OrderByDescending(t => t.CreatedAt)
                .ToList();
        }

        public List<PaymentTransaction> GetTransactionsByCompanyId(int companyId)
        {
            return _context.PaymentTransactions
                .Where(t => t.CompanyId == companyId)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();
        }

        public void ApproveTransaction(int transactionId, int postsToAdd)
        {
            // 1. Find the transaction
            var transaction = _context.PaymentTransactions.Find(transactionId);

            if (transaction != null && transaction.Status == "Pending")
            {
                // 2. Update Transaction Status
                transaction.Status = "Approved";

                // 3. Update CompanyProfile PostsRemaining
                var companyProfile = _context.CompanyProfiles.FirstOrDefault(cp => cp.CompanyId == transaction.CompanyId);
                if (companyProfile != null)
                {
                    companyProfile.PostsRemaining += postsToAdd;
                }

                // 4. Save all changes at once
                _context.SaveChanges();
            }
        }

        public void RejectTransaction(int transactionId)
        {
            var transaction = _context.PaymentTransactions.Find(transactionId);
            if (transaction != null)
            {
                transaction.Status = "Rejected";
                _context.SaveChanges();
            }
        }
    }
}