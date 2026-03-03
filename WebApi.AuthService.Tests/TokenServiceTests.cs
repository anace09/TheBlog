using Microsoft.Extensions.Configuration;
using WebApi.AuthService.Models;
using WebApi.AuthService.Service;

namespace WebApi.AuthService.Tests {

    public class TokenServiceTests
    {

        private readonly TokenService _tokenService;
        private readonly ApplicationUser _testUser;

        public TokenServiceTests() 
        {

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> 
                {
                    ["Jwt:Key"] = "c89710f30ec9b2cb73d836c18b21ed99a624f4b187d1736f926b11e8dcf873b7",
                    ["Jwt:Issuer"] = "AuthService",
                    ["Jwt:Audience"] = "WebApiClients",
                    ["Jwt:ExpireMinutes"] = "60"
                })
                .Build();

            _tokenService = new TokenService(config);

            _testUser = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "testuser"
            };

        }

        // --- GenerateToken ---

        [Fact]
        public void GenerateToken_ValidUser_ReturnsToken() 
        {

            var token = _tokenService.GenerateToken(_testUser, ["User"]);

            Assert.NotNull(token);
            Assert.NotEmpty(token);
        
        }

        [Fact]
        public void GenerateToken_ContainsUserClaims()
        {

            var token = _tokenService.GenerateToken(_testUser, ["User"]);
            var principal = _tokenService.GetPrincipalFromExpiredToken(token);

            Assert.NotNull(principal);
            Assert.Equal(_testUser.UserName, principal.Identity?.Name);

        }

        [Fact]
        public void GenerateToken_ContainsRoles()
        {

            var token = _tokenService.GenerateToken(_testUser, ["Admin", "User"]);
            var principal = _tokenService.GetPrincipalFromExpiredToken(token);

            var roles = principal!.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            Assert.Contains("Admin", roles);
            Assert.Contains("User", roles);

        }

        [Fact]
        public void GenerateToken_NoJwtKey_ThrowsException() 
        {

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>())
                .Build();

            var service = new TokenService(config);

            Assert.Throws<InvalidOperationException>(() =>
                service.GenerateToken(_testUser, []));
        
        }

        // --- GenerateRefreshToken

        [Fact]
        public void GenerateRefreshToken_ReturnsNonEmptyString() 
        {

            var token = _tokenService.GenerateRefreshToken();
            
            Assert.NotNull(token);
            Assert.NotEmpty(token);

        }

        [Fact]
        public void GenerateRefreshToken_EachCallReturnsUniqueToken() 
        {
        
            var token1 = _tokenService.GenerateRefreshToken();
            var token2 = _tokenService.GenerateRefreshToken();

            Assert.NotEqual(token1, token2);
        
        }

        [Fact]
        public void GenerateRefreshToken_IsBase64() 
        {

            var token = _tokenService.GenerateRefreshToken();
            var bytes = Convert.FromBase64String(token);

            Assert.Equal(64, bytes.Length);
        
        }

        [Fact]
        public void GetPrincipalFromExpiredToken_ValidToken_ReturnsPrincipal() 
        {

            var token = _tokenService.GenerateToken(_testUser, ["User"]);
            var principal = _tokenService.GetPrincipalFromExpiredToken(token);

            Assert.NotNull(principal);

        }

        [Fact]
        public void GetPrincipalFromExpiredToken_InvalidToken_ReturnsNull()
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken("invalid.token.here");
            Assert.Null(principal);
        }

    }


}


