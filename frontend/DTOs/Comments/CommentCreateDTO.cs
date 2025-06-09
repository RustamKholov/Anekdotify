using System;
using System.ComponentModel.DataAnnotations;

namespace frontend.Comments.DTOs;

public class CommentCreateDTO
{

    public required string Text { get; set; }
    public int? ParentCommentId { get; set; }
    public int JokeId { get; set; }

}
