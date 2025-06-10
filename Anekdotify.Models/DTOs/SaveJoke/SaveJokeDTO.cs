using System.ComponentModel.DataAnnotations;

namespace Anekdotify.Models.DTOs.SaveJoke
{
    public class SaveJokeDTO
    {
        [Required]
        public int JokeId { get; set; }
    }
}