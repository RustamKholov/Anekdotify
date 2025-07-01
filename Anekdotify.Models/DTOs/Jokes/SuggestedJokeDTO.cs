namespace Anekdotify.Models.DTOs.Jokes
{
    public class SuggestedJokeDto
    {
        public int JokeId { get; set; }
        public string Text { get; set; } = string.Empty;
        public string ClassificationName { get; set; } = string.Empty;

        public bool? IsApproved { get; set; }
    }
}
