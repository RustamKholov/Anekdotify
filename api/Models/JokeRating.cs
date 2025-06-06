using System;
using System.Collections.Generic;

namespace api.Models;

public partial class JokeRating
{
    public int RatingId { get; set; }

    public string UserId { get; set; } = null!;

    public int JokeId { get; set; }

    public bool Rating { get; set; }

    public DateTime RatingDate { get; set; } = DateTime.UtcNow;

    public virtual Joke Joke { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
