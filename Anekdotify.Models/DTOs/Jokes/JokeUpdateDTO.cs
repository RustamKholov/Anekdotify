namespace Anekdotify.Models.DTOs.Jokes
{
    public class JokeUpdateDTO
    {
        public string Text { get; set; } = null!;
        public int? ClassificationId { get; set; }

    }
}