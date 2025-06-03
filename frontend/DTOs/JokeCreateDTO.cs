using System;
using System.ComponentModel.DataAnnotations;

namespace frontend.DTOs;

public class JokeCreateDTO
{
    [Required(ErrorMessage = "Cannot be empty")]
    public required string Title { get; set; } 
    [Required(ErrorMessage = "Cannot be empty")]
    public required string Content { get; set; }
}
