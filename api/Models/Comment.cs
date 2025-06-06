using System;
using System.Collections.Generic;

namespace api.Models;

public partial class Comment
{
    public int CommentId { get; set; }

    public string CommentText { get; set; } = null!;

    public DateTime CommentDate { get; set; } = DateTime.UtcNow;

    public int JokeId { get; set; }

    public string UserId { get; set; } = null!;

    public int? ParentCommentId { get; set; }

    public virtual ICollection<CommentRating> CommentRatings { get; set; } = new List<CommentRating>();

    public virtual Joke Joke { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
