using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using frontend.Models;

namespace frontend.DTOs.Jokes
{
    public class JokeDTO
    {
        public string Text { get; set; } = null!;
        public DateTime SubmissionDate { get; set; }
        public string SourceName { get; set; }
        public int TotalLikes { get; set; }
        public int TotalDislikes { get; set; }
        public string ClassificationName { get; set; }
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}