using System;
using System.Collections.Generic;

namespace api.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Salt { get; set; } = null!;

    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginDate { get; set; }

    public string Role { get; set; } = null!;

    public DateTime? LastJokeRetrievalDate { get; set; }

    public virtual ICollection<CommentRating> CommentRatings { get; set; } = new List<CommentRating>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<JokeLike> JokeLikes { get; set; } = new List<JokeLike>();

    public virtual ICollection<JokeRating> JokeRatings { get; set; } = new List<JokeRating>();

    public virtual ICollection<UserSavedJoke> UserSavedJokes { get; set; } = new List<UserSavedJoke>();
}
