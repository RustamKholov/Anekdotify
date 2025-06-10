namespace Anekdotify.Models.Entities
{
    public class Source
    {
        public int SourceId { get; set; }
        public string SourceName { get; set; } = null!;
        public virtual ICollection<Joke> Jokes { get; set; } = new List<Joke>();
    }
}