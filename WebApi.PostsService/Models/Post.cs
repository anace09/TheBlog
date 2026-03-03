using System.Xml.Linq;

namespace WebApi.PostsService.Models
{
    public class Post
    {

        // --- post content --- 

        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string AuthorId { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set;}

        // --- images loading ---

        public string? CoverImageUrl { get; set; }
        public ICollection<PostImage> Images { get; set; } = [];

        // --- comments ---

        public ICollection<Comment> Comments { get; set; } = [];

        // --- reactions ---

        public ICollection<Reaction> Reactions { get; set; } = [];

        // --- post tags ---

        public ICollection<PostTag> PostTags { get; set; } = [];

        // --- post categories ---

        public ICollection<PostCategory> PostCategories { get; set; } = [];

    }
}
