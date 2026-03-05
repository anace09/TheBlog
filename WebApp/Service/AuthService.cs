using System.Text;
using System.Text.Json;

namespace WebApp.Service
{
    public class AuthService
    {

        private readonly IHttpClientFactory _factory;
        private readonly IHttpContextAccessor _ctx;

        public AuthService(IHttpClientFactory factory, IHttpContextAccessor ctx) {
            _factory = factory;
            _ctx = ctx;
        }

        private HttpClient Client => _factory.CreateClient("Gateway");
        private ISession Session => _ctx.HttpContext!.Session;

        public async Task<bool> LoginAsync(string username, string password) 
        {

            var json = JsonSerializer.Serialize(new { Username = username, Password = password });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync("/api/auth/login", content);
            if (!response.IsSuccessStatusCode) return false;

            var result = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
            Session.SetString("accessToken", result.GetProperty("accessToken").GetString()!);
            Session.SetString("refreshToken", result.GetProperty("refreshToken").GetString()!);
            Session.SetString("username", username);

            var payload = ParseJwt(result.GetProperty("accessToken").GetString()!);
            Session.SetString("role", payload.TryGetProperty("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", out var role) ? role.GetString()! : "");
            Session.SetString("firstName", result.TryGetProperty("firstName", out var fn) ? fn.GetString() ?? "" : "");
            Session.SetString("lastName", result.TryGetProperty("lastName", out var ln) ? ln.GetString() ?? "" : "");

            return true;

        }

        public async Task<bool> RegisterAsync(string username, string email, string password, string firstName, string lastName) 
        {

            var json = JsonSerializer.Serialize(new { Username = username, Email = email, Password = password, FirstName = firstName, LastName = lastName });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync("/api/auth/register", content);

            return response.IsSuccessStatusCode;

        }

        public void Logout() => Session.Clear();

        private JsonElement ParseJwt(string token)
        {
            try
            {
                var payload = token.Split(".")[1];
                var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(padded));
                return JsonSerializer.Deserialize<JsonElement>(json);
            }
            catch { return default; }
        }

    }
}
