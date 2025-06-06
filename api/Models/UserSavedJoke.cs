using System;
using System.Collections.Generic;

namespace api.Models;

public partial class UserSavedJoke
{
    public int UserSavedJokeId { get; set; }

    public string UserId { get; set; } = null!;

    public int JokeId { get; set; }

    public DateTime SavedDate { get; set; } = DateTime.UtcNow;

    public virtual Joke Joke { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
