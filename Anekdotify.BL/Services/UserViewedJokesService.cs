using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anekdotify.BL.Interfaces;
using Anekdotify.Common;

namespace Anekdotify.BL.Services
{
    public class UserViewedJokesService(IUserViewedJokesRepository userViewedJokesRepository) : IUserViewedJokesService
    {
 
        public async Task<OperationResult> AddViewedJokeAsync(string userId, int jokeId)
        {
            return await userViewedJokesRepository.AddViewedJokeAsync(userId, jokeId);
        }

        public async Task<OperationResult<List<int>>> GetViewedJokesAsync(string userId)
        {
            return await userViewedJokesRepository.GetViewedJokesAsync(userId);
        }
    }
}
