using frontend.Models;

namespace frontend.Comments.DTOs;

    public class CommentDTO
    {
        public int CommentId { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public DateTime CommentDate { get; set; }
        public string Username { get; set; } = string.Empty;
        public int? ParentCommentId { get; set; }
        public List<Comment> Replies { get; set; } = new List<Comment>();
        public int TotalLikes { get; set; }
        public int TotalDislikes { get; set; }
    }
