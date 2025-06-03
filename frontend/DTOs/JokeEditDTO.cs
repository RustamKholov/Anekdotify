using System;

namespace frontend.DTOs;

public class JokeEditDTO
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
