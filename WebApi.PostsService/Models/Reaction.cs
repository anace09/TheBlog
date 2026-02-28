namespace WebApi.PostsService.Models
{

    // --- reaction types(add new ones after I finish other parts) ---
    public enum ReactionType { Like, Love, Haha, Sad, Angry }


    public class Reaction
    {

        // --- reaction content --- 

        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public ReactionType Type { get; set; }

        // --- post content
        
        public int PostId { get; set; }

        public Post Post { get; set; } = null!;

    }
}
