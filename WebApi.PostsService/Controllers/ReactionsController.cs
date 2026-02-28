using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApi.PostsService.Data;
using WebApi.PostsService.DTOs;
using WebApi.PostsService.Models;

namespace WebApi.PostsService.Controllers
{

    [ApiController]
    [Route("api/posts/{postId}/reactions")]
    [Authorize]
    public class ReactionsController : ControllerBase
    {

        private readonly AppDbContext _db;
        public ReactionsController(AppDbContext db)
        {
            _db = db;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(int postId) {

            var reactions = await _db.Reactions
                .Where(r => r.PostId == postId)
                .GroupBy(r => r.Type)
                .Select(r => new { Type = r.Key, Count = r.Count() })
                .ToListAsync();

            return Ok(reactions);

        }

        [HttpPost]
        public async Task<IActionResult> Upsert(int postId, [FromBody] UpsertReactionDto request) {

            var existing = _db.Reactions.FirstOrDefault(r => r.PostId == postId && r.UserId == UserId);

            if (existing is not null) existing.Type = request.Type;
            else {
                _db.Reactions.Add( new Reaction
                    {
                    PostId = postId,
                    UserId = UserId,
                    Type = request.Type
                    }
                );
            }

            await _db.SaveChangesAsync();
            return Ok();

        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int postId) {

            var reaction = _db.Reactions.FirstOrDefault( r => r.PostId == postId && r.UserId == UserId);

            if (reaction is null) return NotFound();

            _db.Reactions.Remove(reaction);
            await _db.SaveChangesAsync();
            return NoContent();

        }


    }
}
