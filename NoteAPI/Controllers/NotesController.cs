using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NoteAppApi.Repositories;
using NoteAppApi.Models;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NoteAppApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotesController : ControllerBase
    {
        private readonly NoteRepository _repo;
        private readonly IDatabase _redis;

        public NotesController(NoteRepository repo, IConnectionMultiplexer redis)
        {
            _repo = repo;
            _redis = redis.GetDatabase();
        }

        private int GetUserIdFromHeader()
        {
            var headerUserId = Request.Headers["x-user-id"].ToString();
            if (int.TryParse(headerUserId, out var userId))
            {
                Console.WriteLine($"✅ Extracted userId from header: {userId}");
                return userId;
            }

            Console.WriteLine("❌ Invalid or missing userId in header.");
            return 0;
        }

private async Task<bool> IsAuthorizedFromRedis()
{
    if (!Request.Headers.TryGetValue("x-user-id", out var userIdHeader) || string.IsNullOrWhiteSpace(userIdHeader))
    {
        Console.WriteLine("❌ Missing x-user-id header.");
        return false;
    }

    if (!Request.Headers.TryGetValue("Authorization", out var authHeader) || string.IsNullOrWhiteSpace(authHeader))
    {
        Console.WriteLine("❌ Missing Authorization header.");
        return false;
    }

    if (!int.TryParse(userIdHeader, out int userId))
    {
        Console.WriteLine("❌ Invalid user ID.");
        return false;
    }

    var token = authHeader.ToString().StartsWith("Bearer ")
        ? authHeader.ToString().Substring("Bearer ".Length)
        : authHeader.ToString();

    Console.WriteLine($"Token extracted: {token}");

    // Get token from Redis for this user
    var redisKey = $"auth:{userId}";
    var redisToken = await _redis.StringGetAsync(redisKey);

    if (redisToken.IsNullOrEmpty)
    {
        Console.WriteLine($"❌ No token found in Redis for key {redisKey}.");
        return false;
    }

    Console.WriteLine($"Token in Redis: {redisToken}");

    // Compare tokens
    if (redisToken == token)
    {
        Console.WriteLine("✅ Token matches Redis. Authorization success.");
        return true;
    }
    else
    {
        Console.WriteLine("❌ Token does not match Redis token.");
        return false;
    }
}





        private async Task<bool> IsValidSession(int userId, string jti)
        {
            var redisKey = $"user:{userId}:active";
            var redisValue = await _redis.StringGetAsync(redisKey);
            return !string.IsNullOrEmpty(redisValue) && redisValue == jti;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotes()
        {
            if (!await IsAuthorizedFromRedis())
                return Unauthorized("Invalid session or token mismatch.");

            var userId = int.Parse(Request.Headers["x-user-id"]);

            var notes = await _repo.GetNotesByUserId(userId);
            return Ok(notes);
        }


        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] Note note)
        {
            if (string.IsNullOrWhiteSpace(note.Title))
                return BadRequest("Title is required.");

            var userId = GetUserIdFromHeader();
            if (!await IsAuthorizedFromRedis())
                return Unauthorized("Invalid session or token mismatch.");

            note.UserId = userId;
            note.Id = await _repo.CreateNote(note);
            return CreatedAtAction(nameof(GetNotes), new { id = note.Id }, note);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNote(int id, [FromBody] Note note)
        {
            if (id != note.Id) return BadRequest("Id mismatch.");

            var userId = GetUserIdFromHeader();
            if (!await IsAuthorizedFromRedis())
                return Unauthorized("Invalid session or token mismatch.");

     

            note.UserId = userId;
            var success = await _repo.UpdateNote(note);
            return success ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            var userId = GetUserIdFromHeader();
            if (!await IsAuthorizedFromRedis())
                return Unauthorized("Invalid session or token mismatch.");

            var success = await _repo.DeleteNote(id, userId);
            return success ? NoContent() : NotFound();
        }
    }
}
