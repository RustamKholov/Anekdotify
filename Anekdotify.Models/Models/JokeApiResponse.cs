using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anekdotify.Models.Models
{
    public class JokeApiResponse
    {
        public string? Category { get; set; }
        public string? Type { get; set; }
        public string? Joke { get; set; }
        public int? Id { get; set; }
    }
}
