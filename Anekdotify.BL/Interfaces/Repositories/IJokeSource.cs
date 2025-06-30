using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anekdotify.Models.DTOs.Jokes;

namespace Anekdotify.BL.Interfaces.Repositories
{
    public interface IJokeSource
    {
        Task<JokeCreateDto?> GetJokeAsync();
    }
}
