namespace WebApi.AuthService.Models;

public record RefreshRequest
(
    string AccessToken,
    string RefreshToken
);