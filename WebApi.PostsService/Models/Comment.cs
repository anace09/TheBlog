namespace WebApi.PostsService.Models
{
    public class Comment
    {

        // --- comment content ---

        public int Id { get; set; }
        public string Content { get; set; } = null!;
        public string AuthorId { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --- post content ---
        public int PostId { get; set; }
        public Post Post { get; set; } = null!;

    }
}
