using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anekdotify.Models.Entities
{
    public class RefreshToken
    {
        public int RefreshTokenId { get; set; }
        public string UserId { get; set; } = null!;
        public string Token { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
