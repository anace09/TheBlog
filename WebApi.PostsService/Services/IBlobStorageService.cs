namespace WebApi.PostsService.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadAsync(IFormFile file);
        Task DeleteAsync(string imageUrl);
    }
}