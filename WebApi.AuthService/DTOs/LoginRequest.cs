namespace WebApi.AuthService.Models;

public record LoginRequest(
    string Username,
    string Password
    );