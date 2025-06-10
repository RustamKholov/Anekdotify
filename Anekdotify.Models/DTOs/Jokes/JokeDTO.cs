using Anekdotify.Models.DTOs.Comments;

namespace Anekdotify.Models.DTOs.Jokes
{
    public class JokeDTO
    {
        public int JokeId { get; set; }

        public string Text { get; set; } = null!;

        public DateTime SubmissionDate { get; set; }
        public int? SourceId { get; set; }
        public string? SourceName { get; set; }
        public int TotalLikes { get; set; }
        public int TotalDislikes { get; set; }

        public int? ClassificationId { get; set; }
        public string? ClassificationName { get; set; }
        public List<CommentDTO> Comments { get; set; } = new List<CommentDTO>();
    }
}