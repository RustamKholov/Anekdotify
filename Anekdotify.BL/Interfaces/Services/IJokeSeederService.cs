using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anekdotify.BL.Interfaces.Services
{
    public interface IJokeSeederService
    {
        Task SeedAsync(int count);
    }
}
