using Dapper;
using InternshipManagementSystem.Models;
using System.Data;

namespace InternshipManagementSystem.Data
{
    public class InternshipRepository
    {
        private readonly DbHelper _dbHelper;

        public InternshipRepository(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public int CreateInternship(Internship internship)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                var sql = @"INSERT INTO Internships (Title, Description, RequiredSkills, Location, Duration, Salary, JobType, ClosingDate, CompanyId, CreatedAt) 
                           VALUES (@Title, @Description, @RequiredSkills, @Location, @Duration, @Salary, @JobType, @ClosingDate, @CompanyId, @CreatedAt);
                           SELECT LAST_INSERT_ID();";
                return connection.ExecuteScalar<int>(sql, internship);
            }
        }

        public List<Internship> GetAllInternships()
        {
            using (var connection = _dbHelper.GetConnection())
            {
                var sql = @"SELECT i.*, u.FullName as CompanyName,
                            (SELECT COUNT(*) FROM Applications a WHERE a.InternshipId = i.Id) as ApplicantCount
                            FROM Internships i
                            LEFT JOIN Users u ON i.CompanyId = u.Id
                            ORDER BY i.CreatedAt DESC";
                return connection.Query<Internship>(sql).ToList();
            }
        }

        public List<Internship> GetInternshipsByCompanyId(int companyId)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                return connection.Query<Internship>(
                    "SELECT * FROM Internships WHERE CompanyId = @CompanyId ORDER BY CreatedAt DESC",
                    new { CompanyId = companyId }
                ).ToList();
            }
        }

        public Internship GetInternshipById(int id)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                return connection.QueryFirstOrDefault<Internship>(
                    "SELECT * FROM Internships WHERE Id = @Id",
                    new { Id = id }
                );
            }
        }

        public void UpdateInternship(Internship internship)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                var sql = @"UPDATE Internships 
                   SET Title = @Title, 
                       Description = @Description, 
                       RequiredSkills = @RequiredSkills, 
                       Location = @Location, 
                       Duration = @Duration,
                       Salary = @Salary,
                       JobType = @JobType,
                       ClosingDate = @ClosingDate
                   WHERE Id = @Id";
                connection.Execute(sql, internship);
            }
        }

        public void DeleteInternship(int id)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Execute("DELETE FROM Applications WHERE InternshipId = @Id", new { Id = id });
                connection.Execute("DELETE FROM Internships WHERE Id = @Id", new { Id = id });
            }
        }

        public Dictionary<string, int> GetTopRequiredSkills()
        {
            using var conn = _dbHelper.GetConnection();
            var skills = conn.Query<string>("SELECT RequiredSkills FROM Internships").ToList();

            var skillCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var s in skills)
            {
                var parts = s.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
                foreach (var part in parts)
                    skillCounts[part] = skillCounts.GetValueOrDefault(part, 0) + 1;
            }
            return skillCounts.OrderByDescending(x => x.Value).Take(5).ToDictionary(x => x.Key, x => x.Value);
        }

        public InternshipDetailsViewModel GetInternshipWithCompany(int id)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                var sql = @"SELECT i.Id, i.Title, i.Description, i.RequiredSkills, i.Location, i.Duration, 
                           i.Salary, i.JobType, i.ClosingDate, i.CompanyId, i.CreatedAt,
                           u.FullName as CompanyName, u.Email as CompanyEmail
                   FROM Internships i
                   JOIN Users u ON i.CompanyId = u.Id
                   WHERE i.Id = @Id";
                return connection.QueryFirstOrDefault<InternshipDetailsViewModel>(sql, new { Id = id });
            }
        }
    }
}