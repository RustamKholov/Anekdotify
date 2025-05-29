using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class Joke
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}