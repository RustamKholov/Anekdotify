using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anekdotify.Models.Entities
{
    public class SourceFetchedJoke
    {
        public int SourceFetchedJokeId { get; set; }
        public DateTime FetchedDate { get; set; } = DateTime.UtcNow;
        public int SourceId { get; set; }
        public int SourceJokeId { get; set; }
        public virtual Source Source { get; set; } = null!;

    }
}
