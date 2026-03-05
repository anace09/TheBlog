using Microsoft.AspNetCore.Mvc;
using WebApp.Service;

namespace WebApp.Controllers
{
    public class PostsController : Controller
    {

        private readonly PostsService _postsService;

        public PostsController(PostsService postsService) => _postsService = postsService;

        public async Task<IActionResult> Index() 
        {
        
            var posts = await _postsService.GetPostsAsync();
            return View(posts);

        }

        public async Task<IActionResult> Details(int id)
        {

            var post = await _postsService.GetPostAsync(id);
            if (post is null) return NotFound();
            return View(post);

        }

    }
}
