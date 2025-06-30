using Anekdotify.BL.Helpers;
using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.Entities;
using Newtonsoft.Json;

namespace Anekdotify.BL.Services;

public class JokeService(IJokeRepository jokeRepository, IJokeCacheService jokeCacheService ) : IJokeService
{
    public async Task<Joke> CreateJokeAsync(JokeCreateDto? jokeCreateDto, string userId)
    {
        var joke = await jokeRepository.CreateJokeAsync(jokeCreateDto, userId);

        await jokeCacheService.RemoveAsync("list_jokes");

        return joke;

    }

    public async Task<Joke> DeleteJokeAsync(int id)
    {
        var joke =  await jokeRepository.DeleteJokeAsync(id);

        await jokeCacheService.InvalidateJokeAsync(id);

        return joke;
    }

    public async Task<List<JokeDto>> GetAllJokesAsync(JokesQueryObject query)
    {
        var cacheKey = $"jokes_page{query.PageNumber}_size{query.PageSize}_sort{query.ByDescending}";

        var cached = await jokeCacheService.GetStringAsync(cacheKey);
        if (cached != null)
        {
            return JsonConvert.DeserializeObject<List<JokeDto>>(cached) ?? new List<JokeDto>();
        }

        var jokes = await jokeRepository.GetAllJokesAsync(query);

        // Cache for 5 minutes
        await jokeCacheService.SetStringAsync(cacheKey, JsonConvert.SerializeObject(jokes), TimeSpan.FromMinutes(5));

        return jokes;
    }

    public async Task<List<Comment>> GetCommentsByJokeIdAsync(int jokeId)
    {
        var cacheValue = await jokeCacheService.GetStringAsync($"comments_joke_{jokeId}");

        if (cacheValue != null)
        {
            return JsonConvert.DeserializeObject<List<Comment>>(cacheValue) ?? new List<Comment>();
        }

        var comments = await jokeRepository.GetCommentsByJokeIdAsync(jokeId);

        await jokeCacheService.SetStringAsync($"comments_joke_{jokeId}", JsonConvert.SerializeObject(comments));

        return comments;
    }

    public async Task<JokeDto?> GetJokeByIdAsync(int id)
    {
        var cacheValue = await jokeCacheService.GetStringAsync($"joke_{id}");
        if (cacheValue != null)
        {
            return JsonConvert.DeserializeObject<JokeDto>(cacheValue);
        }
        var joke = await jokeRepository.GetJokeByIdAsync(id);
        if (joke != null)
        {
            await jokeCacheService.SetStringAsync($"joke_{id}", JsonConvert.SerializeObject(joke));
        }
        return joke;
    }

    public async Task<List<JokeDto>> GetJokesByIdsAsync(List<int> ids)
    {
        return await jokeRepository.GetJokesByIdsAsync(ids);
    }

    public async Task<JokeDto> GetRandomJokeAsync(List<int> viewedJokes)
    {
        return await jokeRepository.GetRandomJokeAsync(viewedJokes);
    }

    public async Task<List<JokePreviewDto>> GetSuggestedByMeJokes(string userId)
    {
        return await jokeRepository.GetSuggestedByMeJokes(userId);
    }

    public async Task<bool> JokeExistsAsync(int id)
    {
        return await jokeRepository.JokeExistsAsync(id);
    }

    public async Task<SuggestedJokeDto> SuggestJokeAsync(JokeCreateDto? jokeCreateDto, string userId)
    {
        var joke = await jokeRepository.CreateJokeAsync(jokeCreateDto, userId);
        await jokeCacheService.InvalidateJokeAsync(joke.JokeId);
        return new SuggestedJokeDto
        {
            JokeId = joke.JokeId,
            Text = joke.Text,
            ClassificationName = joke.Classification?.Name ?? "Unknown",
            Status = "Pending" 
        };
    }

    public async Task<Joke> UpdateJokeAsync(int id, JokeUpdateDto? jokeUpdateDto)
    {
        var joke = await jokeRepository.UpdateJokeAsync(id, jokeUpdateDto);

        await jokeCacheService.InvalidateJokeAsync(id);

        return joke;
    }
}
