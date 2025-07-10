using Anekdotify.Models.DTOs.Comments;

namespace Anekdotify.Models.DTOs.Jokes
{
    public class JokeDto
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
        public List<CommentDto> Comments { get; set; } = new List<CommentDto>();
        public bool? IsApproved { get; set; }
    }
}