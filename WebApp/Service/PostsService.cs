using System.Net.Http.Headers;
using System.Text.Json;

namespace WebApp.Service
{
    public class PostsService : HttpClientService
    {

        private readonly IHttpClientFactory _factory;
        private readonly IHttpContextAccessor _ctx;

        public PostsService(IHttpClientFactory factory, IHttpContextAccessor ctx) : base(factory, ctx) { }

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

        public async Task<string?> UploadImageAsync(IFormFile file)
        {
            var client = CreateClient();
            using var form = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            form.Add(fileContent, "file", file.FileName);

            var response = await client.PostAsync("api/posts/upload-image", form);
            if (!response.IsSuccessStatusCode) return null;

            var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
            return json.TryGetProperty("url", out var url) ? url.GetString() : null;
        }

        public async Task<bool> CreatePostAsync(string title, string content, string? coverImageUrl, List<string> imageUrls, List<int> categoryIds, List<int> tagIds)
        {
            var body = JsonSerializer.Serialize(new
            {
                Title = title,
                Content = content,
                CoverImageUrl = coverImageUrl,
                ImageUrls = imageUrls,
                CategoryIds = categoryIds,
                TagIds = tagIds
            });
            var response = await CreateClient().PostAsync("api/posts",
                new StringContent(body, System.Text.Encoding.UTF8, "application/json"));
            return response.IsSuccessStatusCode;
        }

        public async Task<List<JsonElement>> GetCategoriesAsync()
        {
            var response = await CreateClient().GetAsync("api/categories");
            if (!response.IsSuccessStatusCode) return [];
            return JsonSerializer.Deserialize<List<JsonElement>>(await response.Content.ReadAsStringAsync()) ?? [];
        }

        public async Task<List<JsonElement>> GetTagsAsync()
        {
            var response = await CreateClient().GetAsync("api/tags");
            if (!response.IsSuccessStatusCode) return [];
            return JsonSerializer.Deserialize<List<JsonElement>>(await response.Content.ReadAsStringAsync()) ?? [];
        }

    }
}
