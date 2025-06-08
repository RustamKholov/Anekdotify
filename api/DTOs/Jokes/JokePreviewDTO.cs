using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTOs.Jokes
{
    public class JokePreviewDTO
    {
        public int JokeId { get; set; }
        public string Text { get; set; } = null!;
        public string ClassificationName { get; set; } = null!;
        public DateTime SubmissionDate { get; set; }
        public int CommentCount { get; set; }
        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }
        public string Source { get; set; } = null!;

    }
}