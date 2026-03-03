namespace WebApi.PostsService.Models
{
    public class PostImage
    {
        public int Id { get; set; }
        public string Url { get; set; } = null!;
        public int PostId { get; set; }
        public Post Post { get; set; } = null!;
    }
}
