using Microsoft.AspNetCore.Identity;

namespace WebApi.AuthService.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
