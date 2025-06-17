using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.BL.Repositories;
using Anekdotify.Common;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.Entities;
using Newtonsoft.Json;

namespace Anekdotify.BL.Services
{
    public class UserViewedJokesService(IUserViewedJokesRepository userViewedJokesRepository, IJokeService jokeService, IJokeCacheService jokeCacheService) : IUserViewedJokesService
    {
 
        public async Task<OperationResult> AddViewedJokeAsync(string userId, int jokeId)
        {
            await jokeCacheService.InvalidateViewedJokesAsync(userId);
            return await userViewedJokesRepository.AddViewedJokeAsync(userId, jokeId);
        }

        public async Task<OperationResult<JokeDTO>> GetLastViewedJokeAsync(string userId)
        {
            var cacheValue = await jokeCacheService.GetStringAsync($"last_viewed_joke_{userId}");
            if (cacheValue != null)
            {
                var jokeDTOCahce = JsonConvert.DeserializeObject<JokeDTO>(cacheValue);
                if (jokeDTOCahce != null)
                {
                    return OperationResult<JokeDTO>.Success(jokeDTOCahce);
                }
            }
            var lastJokeId = await userViewedJokesRepository.GetLastViewedJokeIdAsync(userId);
            if (lastJokeId == null)
            {
                return OperationResult<JokeDTO>.Fail(new JokeDTO(), "No jokes viewed.");
            }
            var jokeDto = await jokeService.GetJokeByIdAsync(lastJokeId.Value);
            
            if(jokeDto == null)
            {
               return OperationResult<JokeDTO>.Fail(new JokeDTO(), "Joke no longer exists.");
            }

            await jokeCacheService.SetStringAsync($"last_viewed_joke_{userId}", JsonConvert.SerializeObject(jokeDto));

            return OperationResult<JokeDTO>.Success(jokeDto);
        }

        public async Task<OperationResult<List<int>>> GetViewedJokesAsync(string userId)
        {
            var cacheValue = await jokeCacheService.GetStringAsync($"viewed_jokes_{userId}");
            if (cacheValue != null)
            {
                var viewedJokesCache = JsonConvert.DeserializeObject<List<int>>(cacheValue);
                if (viewedJokesCache != null)
                {
                    return OperationResult<List<int>>.Success(viewedJokesCache);
                }
            }
            var viewedJokes = await userViewedJokesRepository.GetViewedJokesAsync(userId);
            if (viewedJokes.IsSuccess && viewedJokes.Value != null)
            {
                await jokeCacheService.SetStringAsync($"viewed_jokes_{userId}", JsonConvert.SerializeObject(viewedJokes.Value));
                return OperationResult<List<int>>.Success(viewedJokes.Value);
            }
            return OperationResult<List<int>>.Fail(new List<int>(), viewedJokes.ErrorMessage!);
        }
        public async Task<OperationResult<bool>> IsLastViewedJokeActualAsync(string userId)
        {
            var cacheValue = await jokeCacheService.GetStringAsync($"is_last_viewed_joke_actual_{userId}");
            if (cacheValue != null)
            {
                var isActualCache = JsonConvert.DeserializeObject<bool>(cacheValue);
                return OperationResult<bool>.Success(isActualCache);
            }
            var res = await userViewedJokesRepository.IsLastViewedJokeActualAsync(userId);
            if (res.IsSuccess)
            {
                await jokeCacheService.SetStringAsync($"is_last_viewed_joke_actual_{userId}", JsonConvert.SerializeObject(res.Value));
                return OperationResult<bool>.Success(res.Value);
            }
            return OperationResult<bool>.Fail(false, res.ErrorMessage!);
        }
    }
}
