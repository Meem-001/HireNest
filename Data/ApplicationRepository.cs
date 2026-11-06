using Dapper;
using InternshipManagementSystem.Models;
using System.Data;

namespace InternshipManagementSystem.Data
{
    public class ApplicationRepository
    {
        private readonly DbHelper _dbHelper;

        public ApplicationRepository(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public bool HasAlreadyApplied(int studentId, int internshipId)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                var count = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Applications WHERE StudentId = @StudentId AND InternshipId = @InternshipId",
                    new { StudentId = studentId, InternshipId = internshipId }
                );
                return count > 0;
            }
        }

        public void CreateApplication(Application application)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Execute(
                    "INSERT INTO Applications (StudentId, InternshipId, Status, AppliedAt, ResumeFileName) VALUES (@StudentId, @InternshipId, @Status, @AppliedAt, @ResumeFileName)",
                    application
                );
            }
        }

        public List<Application> GetApplicationsForCompany(int companyId)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                var sql = @"SELECT a.*, u.FullName as StudentName, u.Email as StudentEmail, i.Title as InternshipTitle 
                           FROM Applications a
                           JOIN Users u ON a.StudentId = u.Id
                           JOIN Internships i ON a.InternshipId = i.Id
                           WHERE i.CompanyId = @CompanyId
                           ORDER BY a.AppliedAt DESC";
                return connection.Query<Application>(sql, new { CompanyId = companyId }).ToList();
            }
        }

        public void UpdateApplicationStatus(int applicationId, string status, string feedback = null)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Execute(
                    "UPDATE Applications SET Status = @Status, Feedback = @Feedback WHERE Id = @Id",
                    new { Id = applicationId, Status = status, Feedback = feedback }
                );
            }
        }

        // ✅ UPDATED: Added JOIN to Users (as Company) to fetch CompanyName
        public List<Application> GetAllApplications()
        {
            using (var connection = _dbHelper.GetConnection())
            {
                var sql = @"SELECT a.*, u.FullName as StudentName, u.Email as StudentEmail, 
                           i.Title as InternshipTitle, i.CompanyId,
                           c.FullName as CompanyName
                   FROM Applications a
                   JOIN Users u ON a.StudentId = u.Id
                   JOIN Internships i ON a.InternshipId = i.Id
                   JOIN Users c ON i.CompanyId = c.Id
                   ORDER BY a.AppliedAt DESC";
                return connection.Query<Application>(sql).ToList();
            }
        }

        public List<Application> GetApplicationsByStudent(int studentId)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                var sql = @"SELECT a.*, i.Title as InternshipTitle, i.Location as InternshipLocation
                           FROM Applications a
                           JOIN Internships i ON a.InternshipId = i.Id
                           WHERE a.StudentId = @StudentId
                           ORDER BY a.AppliedAt DESC";
                return connection.Query<Application>(sql, new { StudentId = studentId }).ToList();
            }
        }

        public void DeleteApplication(int id)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Execute("DELETE FROM Applications WHERE Id = @Id", new { Id = id });
            }
        }

        public Application GetApplicationById(int id)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                var sql = @"SELECT a.*, i.Title as InternshipTitle 
                   FROM Applications a
                   JOIN Internships i ON a.InternshipId = i.Id
                   WHERE a.Id = @Id";
                return connection.QueryFirstOrDefault<Application>(sql, new { Id = id });
            }
        }

        public User GetStudentById(int studentId)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                return connection.QueryFirstOrDefault<User>(
                    "SELECT Id, FullName, Email FROM Users WHERE Id = @Id",
                    new { Id = studentId });
            }
        }

        public Dictionary<string, int> GetApplicationStatusByCompany(int companyId)
        {
            using var conn = _dbHelper.GetConnection();
            var sql = @"SELECT Status, COUNT(*) as Count 
                FROM Applications a 
                JOIN Internships i ON a.InternshipId = i.Id 
                WHERE i.CompanyId = @CompanyId 
                GROUP BY Status";

            var results = conn.Query(sql, new { CompanyId = companyId });
            return results.ToDictionary(row => (string)row.Status, row => (int)row.Count);
        }
    }
}