using System;
using System.Collections.Generic;

namespace api.Models;

public partial class JokePart
{
    public int JokePartId { get; set; }

    public string Text { get; set; } = null!;

    public string PartType { get; set; } = null!;

    public int? AssociatedJokeId { get; set; }

    public bool IsApproved { get; set; }

    public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;

    public virtual Joke? AssociatedJoke { get; set; }
}
