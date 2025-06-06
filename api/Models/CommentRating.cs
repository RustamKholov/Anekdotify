using System;
using System.Collections.Generic;

namespace api.Models;

public partial class CommentRating
{
    public int CommentRateId { get; set; }

    public int UserId { get; set; }

    public int CommentId { get; set; }

    public bool Rating { get; set; }

    public DateTime RatingDate { get; set; } = DateTime.UtcNow;

    public virtual Comment Comment { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
