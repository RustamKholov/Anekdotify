using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTOs.JokeRating
{
    public class JokeRatingDTO
    {
        public int JokeId { get; set; }
        public bool? IsLike { get; set; }

    }
}