using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApi.PostsService.Data;
using WebApi.PostsService.DTOs;
using WebApi.PostsService.Models;
using WebApi.PostsService.Services;

namespace WebApi.PostsService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PostsController : ControllerBase
    {

        private readonly AppDbContext _db;
        public PostsController(AppDbContext db) => _db = db;

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll() 
        {
            var posts = await _db.Posts
                .Include(p => p.Reactions)
                .Include(p => p.Comments)
                .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
                .Include(p => p.PostCategories).ThenInclude(pc => pc.Category)
                .ToListAsync();
            return Ok(posts);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var post = await _db.Posts
                .Include(p => p.Reactions)
                .Include(p => p.Comments)
                .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
                .Include(p => p.PostCategories).ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            return post is null ? NotFound() : Ok(post);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePostDto dto) 
        {

            var post = new Post
            {
                Title = dto.Title,
                Content = dto.Content,
                AuthorId = UserId,
                CoverImageUrl = dto.CoverImageUrl,
                Images = dto.ImageUrls.Select(url => new PostImage { Url = url }).ToList(),
                PostCategories = dto.CategoryIds.Select(id => new PostCategory { CategoryId = id }).ToList(),
                PostTags = dto.TagIds.Select(id => new PostTag { TagId = id }).ToList()
            };

            _db.Posts.Add(post);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = post.Id }, post);

        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(
            IFormFile file,
            [FromServices] IBlobStorageService blobService)
        {

            if (file == null || file.Length == 0)
                return BadRequest("Файл пустой");
             
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!allowed.Contains(ext))
                return BadRequest("Только jpg, png, webp");

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("Максимум 5MB");

            var url = await blobService.UploadAsync(file);
            return Ok(new { url });

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePostDto dto) { 

            var post = await _db.Posts.FindAsync(id);

            // for test: case when post is nil.
            if(post is null) return NotFound();

            // for test: case when id is not matched
            if (post.AuthorId != UserId) return Forbid();

            post.Title = dto.Title;
            post.Content = dto.Content;
            post.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(post);

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id) 
        {

            var task = await _db.Posts.FindAsync(id);

            // for tests: case when task s null and when aId != uId too
            if (task is null) return NotFound();
            if (task.AuthorId != UserId) return Forbid();

            _db.Posts.Remove(task);
            await _db.SaveChangesAsync();
            return NoContent();

        }


    }

}
