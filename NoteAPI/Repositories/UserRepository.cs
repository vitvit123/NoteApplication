using Dapper;
using Microsoft.Data.SqlClient;
using NoteAppApi.Models;

namespace NoteAppApi.Repositories
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        private SqlConnection GetConnection() => new(_connectionString);

        public async Task<User?> GetByUsername(string username)
        {
            using var conn = GetConnection();
            return await conn.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Username = @Username",
                new { Username = username });
        }

        public async Task<int> CreateUser(User user)
        {
            using var conn = GetConnection();
            var sql = @"  
                    INSERT INTO Users (Username, PasswordHash, CreatedAt)  
                    VALUES (@Username, @PasswordHash, GETDATE());  
                    SELECT CAST(SCOPE_IDENTITY() as int);";

            return await conn.ExecuteScalarAsync<int>(sql, user);
        }

        public async Task UpdateUser(User user)
        {
            using var conn = GetConnection(); // Fix: Use the correct connection method  
            var sql = @"  
                    UPDATE Users   
                    SET FailedLoginAttempts = @FailedLoginAttempts, LockoutEnd = @LockoutEnd   
                    WHERE Id = @Id";

            await conn.ExecuteAsync(sql, new { user.FailedLoginAttempts, user.LockoutEnd, user.Id });
        }
    }
}
