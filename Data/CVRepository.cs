using Dapper;
using InternshipManagementSystem.Models;

namespace InternshipManagementSystem.Data
{
    public class CVRepository
    {
        private readonly DbHelper _dbHelper;

        public CVRepository(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public CV GetByStudentId(int studentId)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                // ✅ Explicitly select columns to avoid Dapper mapping issues
                var sql = @"SELECT cv.Id, cv.StudentId, cv.Phone, cv.LinkedIn, cv.Skills, cv.Interests, 
                                   cv.Objective, cv.Education, cv.Experience, cv.PhotoFileName,
                                   u.FullName as StudentName, u.Email as StudentEmail 
                           FROM CVs cv 
                           JOIN Users u ON cv.StudentId = u.Id 
                           WHERE cv.StudentId = @StudentId";

                return connection.QueryFirstOrDefault<CV>(sql, new { StudentId = studentId });
            }
        }

        public void SaveCV(CV cv)
        {
            using (var connection = _dbHelper.GetConnection())
            {
                var existing = GetByStudentId(cv.StudentId);

                if (existing == null)
                {
                    connection.Execute(@"INSERT INTO CVs (StudentId, Phone, LinkedIn, Skills, Interests, Objective, Education, Experience, PhotoFileName) 
                                        VALUES (@StudentId, @Phone, @LinkedIn, @Skills, @Interests, @Objective, @Education, @Experience, @PhotoFileName)", cv);
                }
                else
                {
                    connection.Execute(@"UPDATE CVs SET 
                                        Phone = @Phone, LinkedIn = @LinkedIn, 
                                        Skills = @Skills, Interests = @Interests,
                                        Objective = @Objective, Education = @Education, 
                                        Experience = @Experience, PhotoFileName = @PhotoFileName
                                        WHERE StudentId = @StudentId", cv);
                }
            }
        }
    }
}