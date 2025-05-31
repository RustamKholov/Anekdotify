using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTOs.Comments;
using api.Models;

namespace api.DTOs.Jokes
{
    public class JokeDTO
    {
        public int Id { get; set; }
        public required string Content { get; set; }
        public List<CommentDTO> Comments { get; set; } = new List<CommentDTO>();
    }
}