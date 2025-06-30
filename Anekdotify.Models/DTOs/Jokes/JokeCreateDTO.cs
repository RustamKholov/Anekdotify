using System.ComponentModel.DataAnnotations;

namespace Anekdotify.Models.DTOs.Jokes
{
    public class JokeCreateDto
    {
        [Required]
        [MinLength(5, ErrorMessage = "Content must be 5> characters")]
        [MaxLength(280, ErrorMessage = "Content cannot be over 280 characters")]
        public string Text { get; set; } = string.Empty;
        public int? SourceId { get; set; } = null!;
        public int? ClassificationId { get; set; }
    }
}