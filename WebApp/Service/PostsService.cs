using System.Net.Http.Headers;
using System.Text.Json;

namespace WebApp.Service
{
    public class PostsService
    {

        private readonly IHttpClientFactory _factory;
        private readonly IHttpContextAccessor _ctx;

        public PostsService(IHttpClientFactory factory, IHttpContextAccessor ctx) 
        {

            _factory = factory;
            _ctx = ctx;

        }

        private HttpClient CreateClient() 
        {
            var client = _factory.CreateClient("Gateway");
            var token = _ctx.HttpContext!.Session.GetString("accessToken");
            if (!string.IsNullOrEmpty(token)) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public async Task<List<JsonElement>> GetPostsAsync()
        {

            var response = await CreateClient().GetAsync($"api/posts");
            if (!response.IsSuccessStatusCode) return [];
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<JsonElement>>(json) ?? [];

        }

        public async Task<JsonElement?> GetPostAsync(int id) 
        {

            var response = await CreateClient().GetAsync($"api/posts/{id}");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<JsonElement>(json);


        }

    }
}
