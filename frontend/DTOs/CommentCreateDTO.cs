using System;
using System.ComponentModel.DataAnnotations;

namespace frontend.DTOs;

public class CommentCreateDTO
{
    [Required(ErrorMessage = "Cannot be empty")]
    public required string Title { get; set; }
    [Required(ErrorMessage = "Cannot be empty")]
    public required string Content { get; set; }
    [Required]
    public required int JokeID { get; set; }
}
