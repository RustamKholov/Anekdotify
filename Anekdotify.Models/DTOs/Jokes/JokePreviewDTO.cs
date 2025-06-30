namespace Anekdotify.Models.DTOs.Jokes
{
    public class JokePreviewDto
    {
        public int JokeId { get; set; }
        public string Text { get; set; } = null!;
        public string ClassificationName { get; set; } = null!;
        
        public bool IsApproved { get; set; }
        public DateTime SubmissionDate { get; set; }
        public int CommentCount { get; set; }
        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }
        public string Source { get; set; } = null!;

    }
}