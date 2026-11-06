using Dapper;
using InternshipManagementSystem.Models;
using MySql.Data.MySqlClient;
using System.Data;

namespace InternshipManagementSystem.Data
{
    public class UserRepository
    {
        private readonly DbHelper _dbHelper;

        public UserRepository(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // Check if email already exists
        public bool EmailExists(string email)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                var count = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Users WHERE Email = @Email",
                    new { Email = email }
                );
                return count > 0;
            }
        }

        // Create new user
        public int CreateUser(User user)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                // ✅ Removed PostsRemaining from INSERT
                var sql = @"INSERT INTO Users (FullName, Email, PasswordHash, Role, CreatedAt) 
                           VALUES (@FullName, @Email, @PasswordHash, @Role, @CreatedAt);
                           SELECT LAST_INSERT_ID();";

                var userId = connection.ExecuteScalar<int>(sql, user);

                // ✅ If it's a Company, create CompanyProfile
                if (user.Role == "Company")
                {
                    var profileSql = @"INSERT INTO CompanyProfiles (CompanyId, PostsRemaining, PackageType) 
                                      VALUES (@CompanyId, 5, 'Free')";
                    connection.Execute(profileSql, new { CompanyId = userId });
                }

                return userId;
            }
        }

        // Get user by email
        public User GetUserByEmail(string email)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                return connection.QueryFirstOrDefault<User>(
                    "SELECT * FROM Users WHERE Email = @Email",
                    new { Email = email }
                );
            }
        }

        public List<User> GetAllUsers()
        {
            using (var connection = _dbHelper.GetConnection())
            {
                return connection.Query<User>("SELECT * FROM Users ORDER BY CreatedAt DESC").ToList();
            }
        }

        public List<User> GetUsersByRole(string role)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                return connection.Query<User>(
                    "SELECT * FROM Users WHERE Role = @Role ORDER BY CreatedAt DESC",
                    new { Role = role }
                ).ToList();
            }
        }

        public void DeleteUser(int id)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                // ✅ Also delete CompanyProfile if exists
                connection.Execute("DELETE FROM CompanyProfiles WHERE CompanyId = @Id", new { Id = id });
                connection.Execute("DELETE FROM Users WHERE Id = @Id", new { Id = id });
            }
        }

        // Get user by ID
        public User GetUserById(int id)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                return connection.QueryFirstOrDefault<User>(
                    "SELECT * FROM Users WHERE Id = @Id",
                    new { Id = id }
                );
            }
        }

        public void UpdateUser(User user)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Execute(
                    "UPDATE Users SET FullName = @FullName, Skills = @Skills WHERE Id = @Id",
                    user
                );
            }
        }

        // ✅ DEPRECATED: This method is no longer needed since we use CompanyProfileRepository
        // Keeping it here for backwards compatibility but it does nothing
        public void DecrementPostsRemaining(int userId)
        {
            // This is now handled by CompanyProfileRepository.UpdatePostsRemaining()
            // Keeping this method to avoid breaking existing code
        }
    }
}