namespace Anekdotify.BL.Helpers
{
    public class JokesQueryObject
    {

        public bool ByDescending { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        
    }
}