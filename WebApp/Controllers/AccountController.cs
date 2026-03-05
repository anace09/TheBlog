using Microsoft.AspNetCore.Mvc;
using WebApp.Service;

namespace WebApp.Controllers
{
    public class AccountController : Controller
    {

        private readonly AuthService _authService;

        public AccountController(AuthService authService) => _authService = authService;

        [HttpGet] public IActionResult Login() => View();

        [HttpGet] public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password) 
        {

            if (await _authService.LoginAsync(username, password)) return RedirectToAction("Index", "Posts");
            ViewBag.Error = "Неверный логин или пароль";

            return View();

        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password, string firstName, string lastName) 
        {

            if (await _authService.RegisterAsync(username, email, password, firstName, lastName)) return RedirectToAction("Login");
            ViewBag.Error = "Ошибка регистрации";
            return View();

        }

        public IActionResult Logout()
        {
            _authService.Logout();
            return RedirectToAction("Login");
        }

    }
}
