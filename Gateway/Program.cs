using Microsoft.AspNetCore.Authentication.BearerToken;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddAuthentication(BearerTokenDefaults.AuthenticationScheme)
                .AddBearerToken();

var app = builder.Build();

app.MapGet("login", () =>
    Results.SignIn(
        new ClaimsPrincipal(
            new ClaimsIdentity(
                    [
                    new Claim("sub", Guid.NewGuid().ToString())
                    ],
                    BearerTokenDefaults.AuthenticationScheme
                )
            ),
        authenticationScheme: BearerTokenDefaults.AuthenticationScheme
        )
);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();

app.MapControllers();

app.MapReverseProxy();

app.Run();
