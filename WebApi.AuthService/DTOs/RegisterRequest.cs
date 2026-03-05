namespace WebApi.AuthService.Models;

public record RegisterRequest
(
    string Email,
    string Username,
    string Password,
    string FirstName, 
    string LastName
);