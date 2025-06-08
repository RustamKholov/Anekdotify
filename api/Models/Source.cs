using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class Source
    {
        public int SourceId { get; set; }
        public string SourceName { get; set; } = null!;
        public virtual ICollection<Joke> Jokes { get; set; } = new List<Joke>();
    }
}