namespace WebApi.PostsService.DTOs
{
    public record CreatePostDto(

        string Title,
        string Content,
        string? CoverImageUrl,
        List<string> ImageUrls,
        List<int> CategoryIds,
        List<int> TagIds

    );
}
