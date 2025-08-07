namespace Anekdotify.Models.Entities
{
    public class RefreshToken
    {
        public int RefreshTokenId { get; set; }
        public string UserId { get; set; } = null!;
        public string Token { get; set; } = null!;
        public bool IsRevoked { get; set; }
        public DateTime ExpiryDate { get; set; }
        public virtual User User { get; set; } = null!;

    }
}
