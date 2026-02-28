namespace WebApi.PostsService.Models
{

    // categories list
    public class Category
    {

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<PostCategory> PostCategories { get; set; } = [];

    }

    public class PostCategory
    {

        public int PostId { get; set; }
        public Post Post { get; set; } = null!;
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

    }

}
