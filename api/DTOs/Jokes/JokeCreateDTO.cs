using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTOs.Jokes
{
    public class JokeCreateDTO
    {
        [Required]
        public string Content { get; set; } = string.Empty;
    }
}