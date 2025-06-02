using System;

namespace frontend.DTOs;

public class JokeCreateDTO
{
    public required string Title { get; set; } 
    public required string Content { get; set; }
}
