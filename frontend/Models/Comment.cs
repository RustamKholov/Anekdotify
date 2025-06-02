using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace frontend.Models
{
    public class Comment
    {
        public int ID { get; set; }
        public required string Title { get; set; }
        public required string Content { get; set; }
        public required int JokeID { get; set; }
        public Joke? Joke { get; set; }
    }
}