using System;
using System.Collections.Generic;

namespace api.Models;

public partial class JokeLike
{
    public int LikeId { get; set; }

    public string UserId { get; set; } = null!;

    public int JokeId { get; set; }

    public DateTime LikeDate { get; set; } = DateTime.UtcNow;

    public virtual Joke Joke { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
