using System;

namespace frontend.DTOs;

public class CommentCreateDTO
{
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required int JokeID { get; set; }
}
