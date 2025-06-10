using System.ComponentModel.DataAnnotations;

namespace Anekdotify.Models.DTOs.Comments
{
    public class CommentUpdateDTO
    {

        [MaxLength(280, ErrorMessage = "Content cannot be over 280 characters")]
        public string CommentText { get; set; } = string.Empty;
    }
}