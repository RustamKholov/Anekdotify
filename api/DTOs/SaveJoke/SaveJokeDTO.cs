using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTOs.SaveJoke
{
    public class SaveJokeDTO
    {
        [Required]
        public int JokeId { get; set; }
        [Required]
        public string UserId { get; set; } = null!;
    }
}