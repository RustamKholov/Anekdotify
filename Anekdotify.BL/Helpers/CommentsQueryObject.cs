namespace Anekdotify.BL.Helpers
{
    public class CommentsQueryObject
    {
        public int? JokeId { get; set; } = null;
        public bool ByDescending { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}