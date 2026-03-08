using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WebApp.Service
{
    public class TagsService : HttpClientService
    {

        private readonly IHttpClientFactory _factory;
        private readonly IHttpContextAccessor _ctx;

        public TagsService(IHttpClientFactory factory, IHttpContextAccessor ctx) : base(factory, ctx) { }

        public async Task<List<JsonElement>> GetAllAsync() 
        {

            var response = await CreateClient().GetAsync("/api/tags");
            if (!response.IsSuccessStatusCode) return [];
            
            return JsonSerializer.Deserialize<List<JsonElement>>(await response.Content.ReadAsStringAsync()) ?? [];

        }

        public async Task<bool> CreateAsync(string name) 
        {

            var body = JsonSerializer.Serialize(name);
            var response = await CreateClient().PostAsync("api/tags",
                new StringContent(body, Encoding.UTF8, "application/json"));

            return response.IsSuccessStatusCode;
        
        }

        public async Task<bool> DeleteAsync(int id) 
        {

            var response = await CreateClient().DeleteAsync($"api/tags/{id}");

            return response.IsSuccessStatusCode;
        
        }

    }
}
