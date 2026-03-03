using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using WebApi.AuthService.Controllers;
using WebApi.AuthService.Models;
using WebApi.AuthService.Service;

namespace WebApi.AuthService.Tests;

public class AuthControllerTests
{

    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly TokenService _tokenService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);

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
        _controller = new AuthController(_userManagerMock.Object, _tokenService);
    }

    // --- Login ---

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithTokens() 
    {

        var user = new ApplicationUser { Id = "1", UserName = "testuser" };

        _userManagerMock.Setup(x => x.FindByNameAsync("testuser")).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "password")).ReturnsAsync(true);
        _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(["User"]);
        _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var result = await _controller.Login(new LoginRequest("testuser", "password"));

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);

    }

    [Fact]
    public async Task Login_UserNotFound_ReturnsUnauthorized() 
    {

        _userManagerMock.Setup(x => x.FindByNameAsync("unknown")).ReturnsAsync((ApplicationUser?)null);

        var result = await _controller.Login(new LoginRequest("unknown", "password"));

        Assert.IsType<UnauthorizedObjectResult>(result);

    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {

        var user = new ApplicationUser { Id = "1", UserName = "testuser" };

        _userManagerMock.Setup(x => x.FindByNameAsync("testuser")).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "wrongpassword")).ReturnsAsync(false);

        var result = await _controller.Login(new LoginRequest("testuser", "wrongpassword"));

        Assert.IsType<UnauthorizedObjectResult>(result);

    }


    // --- Register ---

    [Fact]
    public async Task Register_ExistingUser_ReturnsBadRequest() 
    {

        var user = new ApplicationUser { UserName = "existing" };

        _userManagerMock.Setup(x => x.FindByNameAsync("existing")).ReturnsAsync(user);

        var result = await _controller.Register(new RegisterRequest("e@mail.com", "existing", "Password123!"));

        Assert.IsType<BadRequestObjectResult>(result);

    }

    // --- Refresh ---

    [Fact]
    public async Task Refresh_InvalidAccessToken_ReturnsBadRquest() 
    {

        var result = await _controller.Refresh(new RefreshRequest("invalid.token", "refreshtoken"));

        Assert.IsType<BadRequestObjectResult>(result);

    }

    [Fact]
    public async Task Refresh_ValidTokens_ReturnsNewTokens() 
    {

        var user = new ApplicationUser { Id = "1", UserName = "testuser" };
        var accessToken = _tokenService.GenerateToken(user, ["User"]);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        _userManagerMock.Setup(x => x.FindByNameAsync("testuser")).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(["User"]);
        _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var result = await _controller.Refresh(new RefreshRequest(accessToken, refreshToken));

        Assert.IsType<OkObjectResult>(result);

    }

}