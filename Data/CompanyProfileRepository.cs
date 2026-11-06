using Dapper;
using InternshipManagementSystem.Models;

namespace InternshipManagementSystem.Data
{
    public class CompanyProfileRepository
    {
        private readonly DbHelper _dbHelper;

        public CompanyProfileRepository(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public CompanyProfile GetByCompanyId(int companyId)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                var sql = "SELECT * FROM CompanyProfiles WHERE CompanyId = @CompanyId";
                return connection.QueryFirstOrDefault<CompanyProfile>(sql, new { CompanyId = companyId });
            }
        }

        public void CreateCompanyProfile(CompanyProfile profile)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                var sql = @"INSERT INTO CompanyProfiles (CompanyId, PostsRemaining, Website, CompanyDescription, Industry, Location, PackageType)
                           VALUES (@CompanyId, @PostsRemaining, @Website, @CompanyDescription, @Industry, @Location, @PackageType)";
                connection.Execute(sql, profile);
            }
        }

        public void UpdatePostsRemaining(int companyId, int postsRemaining)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                var sql = "UPDATE CompanyProfiles SET PostsRemaining = @PostsRemaining WHERE CompanyId = @CompanyId";
                connection.Execute(sql, new { CompanyId = companyId, PostsRemaining = postsRemaining });
            }
        }

        public void UpdateCompanyProfile(CompanyProfile profile)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                var sql = @"UPDATE CompanyProfiles SET 
                           Website = @Website, 
                           CompanyDescription = @CompanyDescription, 
                           Industry = @Industry, 
                           Location = @Location,
                           PackageType = @PackageType
                           WHERE CompanyId = @CompanyId";
                connection.Execute(sql, profile);
            }
        }
    }
}