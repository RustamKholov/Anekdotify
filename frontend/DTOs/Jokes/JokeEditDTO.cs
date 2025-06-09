using System;

namespace frontend.Jokes.DTOs;

public class JokeEditDTO
{
    public string Text { get; set; } = null!;
    public int? ClassificationId { get; set; }
}
