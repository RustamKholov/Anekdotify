using System.ComponentModel.DataAnnotations;

namespace Anekdotify.Models.DTOs.Accounts
{
    public class LoginDTO
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}