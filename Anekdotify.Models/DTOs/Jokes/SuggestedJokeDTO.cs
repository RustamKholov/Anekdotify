using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anekdotify.Models.DTOs.Jokes
{
    public class SuggestedJokeDTO
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string ClassificationName { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
    }
}
