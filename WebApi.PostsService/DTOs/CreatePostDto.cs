namespace WebApi.PostsService.DTOs
{
    public class CreatePostDto
    {

        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public List<int> CategoryIds { get; set; } = [];
        public List<int> TagIds { get; set; } = [];

    }
}
