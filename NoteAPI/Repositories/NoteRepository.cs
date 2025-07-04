using Dapper;
using Microsoft.Data.SqlClient;
using NoteAppApi.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NoteAppApi.Repositories
{
    public class NoteRepository
    {
        private readonly string _connectionString;

        public NoteRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        private SqlConnection GetConnection() => new(_connectionString);

        public async Task<List<Note>> GetNotesByUserId(int userId)
        {
            var sql = "SELECT * FROM Notes WHERE UserId = @UserId AND deleted_at IS NULL ORDER BY CreatedAt DESC";
            using var conn = new SqlConnection(_connectionString);
            return (await conn.QueryAsync<Note>(sql, new { UserId = userId })).ToList();
        }

        public async Task<int> CreateNote(Note note)
        {
            using var conn = GetConnection();
            var sql = @"
                INSERT INTO Notes (UserId, Title, Content, CreatedAt, UpdatedAt)
                VALUES (@UserId, @Title, @Content, GETDATE(), GETDATE());
                SELECT CAST(SCOPE_IDENTITY() as int);";

            return await conn.ExecuteScalarAsync<int>(sql, note);
        }

        public async Task<bool> UpdateNote(Note note)
        {
            using var conn = GetConnection();
            var sql = @"
                UPDATE Notes
                SET Title = @Title,
                    Content = @Content,
                    UpdatedAt = GETDATE()
                WHERE Id = @Id AND UserId = @UserId";

            var rows = await conn.ExecuteAsync(sql, note);
            return rows > 0;
        }

        public async Task<bool> DeleteNote(int id, int userId)
        {
            using var conn = GetConnection();
            var sql = @"
        UPDATE Notes
        SET deleted_at = GETDATE(),  
            delby = @UserId     
        WHERE Id = @Id AND UserId = @UserId AND deleted_at IS NULL";

            var rows = await conn.ExecuteAsync(sql, new { Id = id, UserId = userId });
            return rows > 0;
        }


    }
}
