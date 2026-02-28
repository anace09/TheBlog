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
    [Route("api/posts/{postId}/comments")]
    [Authorize]
    public class CommentsController : ControllerBase
    {

        private readonly AppDbContext _db;
        public CommentsController(AppDbContext db)
        {
            _db = db;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(int postId)
        {

            var comments = await _db.Comments
                .Where(c => c.PostId == postId)
                .ToListAsync();
            return Ok(comments);

        }
        
        [HttpPost]
        public async Task<IActionResult> Create(int postId, [FromBody] CreateCommentDto request) {

            var post = await _db.Posts.FindAsync(postId);

            if (post == null) return NotFound();

            var comment = new Comment
            {

                Content = request.Content,
                AuthorId = UserId,
                CreatedAt = DateTime.UtcNow,

                PostId = post.Id,
                Post = post

            };
            
            _db.Comments.Add(comment);
            await _db.SaveChangesAsync();
            return Ok(comment);

        }

        [HttpDelete("{commentId}")]
        public async Task<IActionResult> Delete(int postId, int commentId) {

            var comment = await _db.Comments.FindAsync(commentId);
            if(comment == null) return NotFound();
            if (UserId == comment.AuthorId) return Forbid();

            _db.Comments.Remove(comment);
            await _db.SaveChangesAsync();
            return Ok(comment);
        
        }

    }
}
