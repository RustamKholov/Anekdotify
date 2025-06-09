using System;
using System.ComponentModel.DataAnnotations;

namespace frontend.Comments.DTOs;

public class CommentEditDTO
{
    public string Text { get; set; } = string.Empty;
}

