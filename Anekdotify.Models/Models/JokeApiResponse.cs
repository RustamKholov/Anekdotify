namespace Anekdotify.Models.Models
{
    public class JokeApiResponse
    {
        public string? Category { get; set; }
        public string? Type { get; set; }
        public string? Joke { get; set; }
        public string? Setup { get; set; }
        public string? Delivery { get; set; }  
        public int? Id { get; set; }
    }
}
