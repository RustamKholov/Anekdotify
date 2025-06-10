using System.ComponentModel.DataAnnotations;

namespace Anekdotify.Models.DTOs.Accounts
{
    public class RegisterDTO
    {
        [Required]
        public string? Username { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}