using System.Text;
using System.Text.Json;

namespace WebApp.Service
{
    public class CategoriesService : HttpClientService
    {

        public CategoriesService(IHttpClientFactory factory, IHttpContextAccessor ctx) : base(factory, ctx) {}

        public async Task<List<JsonElement>> GetAllAsync() 
        {

            var response = await CreateClient().GetAsync("api/categories");
            if (!response.IsSuccessStatusCode) return [];

            return JsonSerializer.Deserialize<List<JsonElement>>(await response.Content.ReadAsStringAsync()) ?? [];
        
        }

        public async Task<bool> CreateAsync(string name) 
        {

            var body = JsonSerializer.Serialize(name);
            var response = await CreateClient().PostAsync("api/categories", 
                new StringContent(body, Encoding.UTF8, "application/json"));

            return response.IsSuccessStatusCode;
        
        }

        public async Task<bool> DeleteAsync(int id) 
        {
        
            var response = await CreateClient().DeleteAsync($"api/categories/{id}");

            return response.IsSuccessStatusCode;

        }

    }
}
