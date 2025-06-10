namespace Anekdotify.Models.DTOs.Comments
{
    public class CommentDTO
    {
        public int CommentId { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public DateTime CommentDate { get; set; }
        public int JokeId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int? ParentCommentId { get; set; }

        public List<CommentDTO> Replies { get; set; } = new List<CommentDTO>();

        public int TotalLikes { get; set; }

        public int TotalDislikes { get; set; }
        

    }
}