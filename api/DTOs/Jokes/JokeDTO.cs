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
        public int Id { get; set; }
        
        [Required]
        [MinLength(5, ErrorMessage = "Title must be 5> characters")]
        [MaxLength(280, ErrorMessage = "Title cannot be over 280 characters")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MinLength(5, ErrorMessage = "Content must be 5> characters")]
        [MaxLength(280, ErrorMessage = "Content cannot be over 280 characters")]
        public required string Content { get; set; }
        public List<CommentDTO> Comments { get; set; } = new List<CommentDTO>();
    }
}