using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using api.DTOs.Comments;
using api.Models;

namespace api.DTOs.Jokes
{
    public class JokeDTO
    {
        public int JokeId { get; set; }

        public string Text { get; set; } = null!;

        public DateTime SubmissionDate { get; set; }
        public string Source { get; set; } = null!;

        public int TotalLikes { get; set; }
        public int TotalDislikes { get; set; }

        public int? ClassificationId { get; set; }
        public string? ClassificationName { get; set; }
        public List<CommentDTO> Comments { get; set; } = new List<CommentDTO>();
    }
}