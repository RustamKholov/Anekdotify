using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace api.Models;

public partial class User : IdentityUser
{

    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginDate { get; set; }

    public DateTime? LastJokeRetrievalDate { get; set; }

    public virtual ICollection<CommentRating> CommentRatings { get; set; } = new List<CommentRating>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<JokeLike> JokeLikes { get; set; } = new List<JokeLike>();

    public virtual ICollection<JokeRating> JokeRatings { get; set; } = new List<JokeRating>();

    public virtual ICollection<UserSavedJoke> UserSavedJokes { get; set; } = new List<UserSavedJoke>();
}
