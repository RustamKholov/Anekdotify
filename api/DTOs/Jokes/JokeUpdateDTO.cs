using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTOs.Jokes
{
    public class JokeUpdateDTO
    {
        public string Text { get; set; } = null!;
        public string Source { get; set; } = null!;
        public int? ClassificationId { get; set; }

    }
}