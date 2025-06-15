using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;

namespace Anekdotify.BL.Services
{
    public class JokeSeederService(IJokeService jokeService, IJokeSource jokeSource) : IJokeSeederService
    {
        public async Task SeedAsync(int count = 50)
        {
            for (int i = 0; i < count; i++)
            {
                var joke = await jokeSource.GetJokeAsync();
                if (joke != null)
                {
                    await jokeService.CreateJokeAsync(joke, "031dd7c0-ed41-443c-9b43-61a5b2d75cd4"); // My user ID for seeding purposes
                }
                else
                {
                    Console.WriteLine("Failed to fetch a joke from the source.");
                }
            }
        }
    }
}
