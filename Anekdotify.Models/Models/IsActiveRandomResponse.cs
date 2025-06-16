using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anekdotify.Models.Models
{
    public class IsActiveRandomResponse
    {
        public bool IsActive { get; set; }
        public DateTime NextRandomJokeAvailableAt { get; set; }
    }
}
