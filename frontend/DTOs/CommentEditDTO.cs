using System;
using System.ComponentModel.DataAnnotations;

namespace frontend.DTOs;

public class CommentEditDTO
{

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;
}

