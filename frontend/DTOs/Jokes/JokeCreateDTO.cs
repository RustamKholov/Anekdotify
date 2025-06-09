using System;
using System.ComponentModel.DataAnnotations;

namespace frontend.Jokes.DTOs;

public class JokeCreateDTO
{
    public string Text { get; set; } = null!;
    public int? SourceId { get; set; }
    public int? ClassificationId { get; set; }
}
