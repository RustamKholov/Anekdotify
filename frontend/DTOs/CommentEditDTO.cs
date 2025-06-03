using System;
using System.ComponentModel.DataAnnotations;

namespace frontend.DTOs;

public class CommentEditDTO
{
    public int? Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;
}

