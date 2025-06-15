using Anekdotify.BL.Helpers;
using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Anekdotify.BL.Services;

public class JokeService(IJokeRepository jokeRepository, IDistributedCache cacheService ) : IJokeService
{
    public async Task<Joke> CreateJokeAsync(JokeCreateDTO jokeCreateDTO, string userId)
    {
        var joke = await jokeRepository.CreateJokeAsync(jokeCreateDTO, userId);
        await cacheService.RemoveAsync("list_jokes");
        return joke;
    }

    public async Task<Joke> DeleteJokeAsync(int id)
    {
        var joke =  await jokeRepository.DeleteJokeAsync(id);

        await cacheService.RemoveAsync("list_jokes");
        await cacheService.RemoveAsync($"joke_{id}");
        await cacheService.RemoveAsync($"comments_joke_{id}");

        return joke;
    }

    public async Task<List<JokeDTO>> GetAllJokesAsync(JokesQueryObject query)
    {
        var cacheValue = await cacheService.GetStringAsync("list_jokes");
        if (cacheValue != null)
        {
            return JsonConvert.DeserializeObject<List<JokeDTO>>(cacheValue) ?? new List<JokeDTO>();
        }
        var jokes =  await jokeRepository.GetAllJokesAsync(query);
        await cacheService.SetStringAsync("list_jokes", JsonConvert.SerializeObject(jokes));
        return jokes;
    }

    public async Task<List<Comment>> GetCommentsByJokeIdAsync(int jokeId)
    {
        var cacheValue = await cacheService.GetStringAsync($"comments_joke_{jokeId}");

        if (cacheValue != null)
        {
            return JsonConvert.DeserializeObject<List<Comment>>(cacheValue) ?? new List<Comment>();
        }

        var comments = await jokeRepository.GetCommentsByJokeIdAsync(jokeId);

        await cacheService.SetStringAsync($"comments_joke_{jokeId}", JsonConvert.SerializeObject(comments));

        return comments;
    }

    public async Task<JokeDTO?> GetJokeByIdAsync(int id)
    {
        var cacheValue = await cacheService.GetStringAsync($"joke_{id}");
        if (cacheValue != null)
        {
            return JsonConvert.DeserializeObject<JokeDTO>(cacheValue);
        }
        var joke = await jokeRepository.GetJokeByIdAsync(id);
        if (joke != null)
        {
            await cacheService.SetStringAsync($"joke_{id}", JsonConvert.SerializeObject(joke));
        }
        return joke;
    }

    public async Task<JokeDTO> GetRandomJokeAsync(List<int> viewedJokes)
    {
        return await jokeRepository.GetRandomJokeAsync(viewedJokes);
    }

    public async Task<bool> JokeExistsAsync(int id)
    {
        return await jokeRepository.JokeExistsAsync(id);
    }

    public async Task<Joke> UpdateJokeAsync(int id, JokeUpdateDTO jokeUpdateDTO)
    {
        var joke = await jokeRepository.UpdateJokeAsync(id, jokeUpdateDTO);

        await cacheService.RemoveAsync("list_jokes");
        await cacheService.RemoveAsync($"joke_{id}");
        await cacheService.RemoveAsync($"comments_joke_{id}");

        return joke;
    }
}
