namespace Anekdotify.Models.DTOs.Accounts
{
    public class NewUserDTO
    {
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public long ExpiresIn { get; set; } // in seconds

    }
}