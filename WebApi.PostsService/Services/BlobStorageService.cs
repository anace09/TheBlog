using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace WebApi.PostsService.Services
{
    public class BlobStorageService : IBlobStorageService
    {

        private readonly BlobContainerClient _container;

        public BlobStorageService(IConfiguration config) {
            var connectionString = config["AzureBlob:ConnectionString"]!;
            var containerName = config["AzureBlob:ContainerName"]!;
            _container = new BlobContainerClient(connectionString, containerName);
        }

        public async Task<string> UploadAsync(IFormFile file)
        {

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var blob = _container.GetBlobClient(fileName);

            await blob.UploadAsync(file.OpenReadStream(), new BlobHttpHeaders
            {
                ContentType = file.ContentType
            });
        
            return blob.Uri.ToString();

        }

        public async Task DeleteAsync(string imageUrl) 
        {
        
            var uri = new Uri(imageUrl);
            var fileName = Path.GetFileName(uri.LocalPath);
            var blob = _container.GetBlobClient(fileName);
            await blob.DeleteAsync();

        }

    }
}
