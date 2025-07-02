namespace Anekdotify.BL.Helpers;

public class RandomJokeQueryObject
{
    public List<int> ClassificationIds { get; set; } = new List<int>();
    public List<int> SourceIds { get; set; } = new List<int>();
}