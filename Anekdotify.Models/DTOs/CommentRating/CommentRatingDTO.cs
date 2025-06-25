using System;

namespace Anekdotify.Models.DTOs.CommentRating;

public class CommentRatingDTO
{
    public int CommentId { get; set; }
    public bool? IsLike { get; set; }
}
