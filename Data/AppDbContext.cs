using Microsoft.EntityFrameworkCore;
using InternshipManagementSystem.Models;

namespace InternshipManagementSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Internship> Internships { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<CV> Cvs { get; set; }
        public DbSet<CompanyProfile> CompanyProfiles { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Map to your exact MySQL table names
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Internship>().ToTable("internships");
            modelBuilder.Entity<Application>().ToTable("applications");
            modelBuilder.Entity<CV>().ToTable("cvs");
            modelBuilder.Entity<CompanyProfile>().ToTable("companyprofiles");
            modelBuilder.Entity<PaymentTransaction>().ToTable("transactions");

            // ✅ FIX: Tell EF that the link between User and PaymentTransaction is CompanyId
            modelBuilder.Entity<PaymentTransaction>()
                .HasOne<User>() // A transaction belongs to one User (Company)
                .WithMany(u => u.Transactions) // A User has many transactions
                .HasForeignKey(pt => pt.CompanyId) // The foreign key is CompanyId, NOT UserId
                .OnDelete(DeleteBehavior.ClientSetNull);

            base.OnModelCreating(modelBuilder);
        }
    }
}