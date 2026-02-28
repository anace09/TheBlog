using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.PostsService.Data;
using WebApi.PostsService.Models;

namespace WebApi.PostsService.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class TagsController : ControllerBase
    {

        private readonly AppDbContext _db;

        public TagsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() {

            var tags = await _db.Tags.ToListAsync();
            return Ok(tags);

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id) {

            var tag = await _db.Tags.FindAsync(id);
            return tag is null ? NotFound() : Ok(tag);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] string name) 
        {

            var tag = new Tag
            {
                Name = name
            };
            _db.Tags.Add(tag);
            await _db.SaveChangesAsync();
            return Ok(tag);

        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> Delete(int id) 
        {

            var tag = await _db.Tags.FindAsync(id);

            if(tag is null) return NotFound();
            _db.Tags.Remove(tag);
            await _db.SaveChangesAsync();

            return Ok(tag);

        }

    }
}
