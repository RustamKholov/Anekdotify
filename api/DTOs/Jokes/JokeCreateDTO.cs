using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTOs.Jokes
{
    public class JokeCreateDTO
    {
        [Required]
        [MinLength(5, ErrorMessage = "Content must be 5> characters")]
        [MaxLength(280, ErrorMessage = "Content cannot be over 280 characters")]
        public string Text { get; set; } = string.Empty;
        public int? SourceId { get; set; } = null!;
        public int? ClassificationId { get; set; }
    }
}