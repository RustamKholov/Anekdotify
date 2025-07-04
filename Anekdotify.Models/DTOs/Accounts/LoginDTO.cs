using System.ComponentModel.DataAnnotations;

namespace Anekdotify.Models.DTOs.Accounts
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Fiels is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Fiels is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}