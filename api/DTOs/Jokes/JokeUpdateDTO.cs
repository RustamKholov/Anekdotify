using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTOs.Jokes
{
    public class JokeUpdateDTO
    {
        public required string Content { get; set; }
    }
}