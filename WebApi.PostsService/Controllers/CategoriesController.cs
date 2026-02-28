using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.PostsService.Data;

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

        public async Task<IActionResult> GetAll() {

            var categories = await _db.Categories.ToListAsync();
            return Ok(categories);

        }

        // instructions: getByID httget[id] int id
        // instructions: var category find if null notFound else ok(categ)

        // instructions: post authorize Create from body string name
        // instructions: var category = new + add + save

        // instructions: delete authorizo int id
        // instructions: var category FindASync
        // instructions: if category == null return notFound()
        // instructions:  delete save nocon


    }
}
