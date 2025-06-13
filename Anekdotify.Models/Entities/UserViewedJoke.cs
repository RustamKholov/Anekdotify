using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anekdotify.Models.Entities
{
    public partial class UserViewedJoke
    {
        public int UserViewedJokeId { get; set; }

        public string UserId { get; set; } = null!;

        public int JokeId { get; set; }

        public DateTime ViewedDate { get; set; } = DateTime.UtcNow;

        public virtual Joke Joke { get; set; } = null!;

        public virtual User User { get; set; } = null!;
    }
}
