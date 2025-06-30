using System.ComponentModel.DataAnnotations;

namespace Anekdotify.Models.DTOs.SaveJoke
{
    public class SaveJokeDto
    {
        [Required]
        public int JokeId { get; set; }
    }
}