namespace Anekdotify.Models.DTOs.Jokes
{
    public class JokeUpdateDto
    {
        public string Text { get; set; } = null!;
        public int? ClassificationId { get; set; }

    }
}