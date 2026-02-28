namespace WebApi.AuthService.Models;

public class RegisterRequest
{
    public string Email { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
}