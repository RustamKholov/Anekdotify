using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;

namespace api.DTOs.Comments
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