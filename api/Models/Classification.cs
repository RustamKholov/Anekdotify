using System;
using System.Collections.Generic;

namespace api.Models;

public partial class Classification
{
    public int ClassificationId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Joke> Jokes { get; set; } = new List<Joke>();
}
