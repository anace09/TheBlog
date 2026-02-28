using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using WebApi.PostsService.Data;
using WebApi.PostsService.Models;

namespace WebApi.PostsService.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {

        private readonly AppDbContext _db;

        public CategoriesController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {

            var categories = await _db.Categories.ToListAsync();
            return Ok(categories);

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {

            var category = await _db.Categories.FindAsync(id);
            return category is null ? NotFound() : Ok(category);

        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] string name)
        {

            var category = new Category
            {
                Name = name
            };
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
            return Ok(category);

        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {

            var category = await _db.Categories.FindAsync(id);
            if (category == null) return NotFound();
            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
            return NoContent();

        }
    }
}
