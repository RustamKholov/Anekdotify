namespace Anekdotify.Models.Entities
{
    public partial class Joke
    {
        public int JokeId { get; set; }

        public string Text { get; set; } = null!;

        public DateTime SubbmissionDate { get; set; } = DateTime.UtcNow;

        public int SourceId { get; set; }

        public bool IsApproved { get; set; }

        public string? SubbmitedByUserId { get; set; }

        public DateTime? ApprovalDate { get; set; }

        public string? ApprovedByUserId { get; set; } = null!;

        public int? ClassificationId { get; set; }

        public virtual Classification? Classification { get; set; }

        public virtual Source? Source { get; set; }

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public virtual ICollection<JokeLike> JokeLikes { get; set; } = new List<JokeLike>();

        public virtual ICollection<JokePart> JokeParts { get; set; } = new List<JokePart>();

        public virtual ICollection<JokeRating> JokeRatings { get; set; } = new List<JokeRating>();

        public virtual ICollection<UserSavedJoke> UserSavedJokes { get; set; } = new List<UserSavedJoke>();

        public virtual ICollection<UserViewedJoke> UserViewedJokes { get; set; } = new List<UserViewedJoke>();
    }
}
