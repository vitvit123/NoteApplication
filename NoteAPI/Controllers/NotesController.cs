using Microsoft.AspNetCore.Mvc;
using NoteAppApi.Repositories;
using NoteAppApi.Models;
using System.Threading.Tasks;

namespace NoteAppApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotesController : ControllerBase
    {
        private readonly NoteRepository _repo;

        public NotesController(NoteRepository repo)
        {
            _repo = repo;
        }

        // This now always returns a fixed userId (for testing)
        private int GetUserId()
        {
            // For testing: hard-code or skip userId check entirely
            return 6;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotes()
        {
            var userId = GetUserId();

            // Optional: Log or debug
            Console.WriteLine($"Fetching notes for userId = {userId}");

            // Calls repository which does WHERE UserId = @userId
            var notes = await _repo.GetNotesByUserId(userId);

            // Return filtered notes
            return Ok(notes);
        }


        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] Note note)
        {
            if (string.IsNullOrWhiteSpace(note.Title))
                return BadRequest("Title is required.");

            var userId = GetUserId();

            note.UserId = userId;
            var newId = await _repo.CreateNote(note);
            note.Id = newId;

            return CreatedAtAction(nameof(GetNotes), new { id = newId }, note);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNote(int id, [FromBody] Note note)
        {
            if (id != note.Id) return BadRequest("Id mismatch.");

            var userId = GetUserId();
            // No Unauthorized check

            note.UserId = userId;

            var success = await _repo.UpdateNote(note);
            if (!success) return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            var userId = GetUserId();
            // No Unauthorized check

            var success = await _repo.DeleteNote(id, userId);
            if (!success) return NotFound();

            return NoContent();
        }
    }
}
