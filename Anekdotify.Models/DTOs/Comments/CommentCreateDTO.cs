using System.ComponentModel.DataAnnotations;

namespace Anekdotify.Models.DTOs.Comments
{
    public class CommentCreateDTO
    {

        [Required]
        [MinLength(5, ErrorMessage = "Content must be 5> characters")]
        [MaxLength(280, ErrorMessage = "Content cannot be over 280 characters")]
        public string CommentText { get; set; } = string.Empty;
        public int? ParentCommentId { get; set; }

    }
}