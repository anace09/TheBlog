namespace WebApi.PostsService.Models
{
    public class Tag
    {

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<PostTag> PostTags { get; set; } = [];

    }

    public class PostTag
    {

        public int PostId { get; set; }
        public Post Post { get; set; } = null!;
        public int TagId { get; set; }
        public Tag Tag { get; set; } = null!;

    }

}
