using Anekdotify.BL.Helpers;
using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.Entities;
using Microsoft.Extensions.Logging;

namespace Anekdotify.BL.Services;

public class JokeService : IJokeService
{
    private readonly IJokeRepository _jokeRepository;
    private readonly IJokeCacheService _jokeCacheService;
    private readonly ILogger<JokeService> _logger;

    public JokeService(IJokeRepository jokeRepository, IJokeCacheService jokeCacheService, ILogger<JokeService> logger)
    {
        _jokeRepository = jokeRepository;
        _jokeCacheService = jokeCacheService;
        _logger = logger;
    }

    public async Task<Joke> CreateJokeAsync(JokeCreateDto? jokeCreateDto, string userId)
    {
        var joke = await _jokeRepository.CreateJokeAsync(jokeCreateDto, userId);
        await _jokeCacheService.InvalidateJokeAsync(joke.JokeId);
        _logger.LogInformation("Created joke {JokeId} by user {UserId} and invalidated cache.", joke.JokeId, userId);
        return joke;
    }

    public async Task<Joke> DeleteJokeAsync(int id)
    {
        var joke = await _jokeRepository.DeleteJokeAsync(id);
        await _jokeCacheService.InvalidateJokeAsync(id);
        _logger.LogInformation("Deleted joke {JokeId} and invalidated cache.", id);
        return joke;
    }

    public async Task<List<JokeDto>> GetAllJokesAsync(JokesQueryObject query)
    {
        var cacheKey = JokeCacheKeys.JokesList(query.PageNumber, query.PageSize, query.ByDescending);
        var cached = await _jokeCacheService.GetAsync<List<JokeDto>>(cacheKey);
        if (cached != null)
        {
            _logger.LogInformation("Returned jokes from cache for key {CacheKey}", cacheKey);
            return cached;
        }

        var jokes = await _jokeRepository.GetAllJokesAsync(query);
        await _jokeCacheService.SetAsync(cacheKey, jokes, TimeSpan.FromMinutes(5));
        _logger.LogInformation("Fetched jokes from repository and cached for key {CacheKey}", cacheKey);
        return jokes;
    }

    public async Task<List<Comment>> GetCommentsByJokeIdAsync(int jokeId)
    {
        var cacheKey = JokeCacheKeys.Comments(jokeId);
        var cached = await _jokeCacheService.GetAsync<List<Comment>>(cacheKey);
        if (cached != null)
        {
            _logger.LogInformation("Returned comments from cache for joke {JokeId}", jokeId);
            return cached;
        }

        var comments = await _jokeRepository.GetCommentsByJokeIdAsync(jokeId);
        await _jokeCacheService.SetAsync(cacheKey, comments);
        _logger.LogInformation("Fetched comments from repository and cached for joke {JokeId}", jokeId);
        return comments;
    }

    public async Task<JokeDto?> GetJokeByIdAsync(int id)
    {
        var cacheKey = JokeCacheKeys.Joke(id);
        var cached = await _jokeCacheService.GetAsync<JokeDto>(cacheKey);
        if (cached != null)
        {
            _logger.LogInformation("Returned joke from cache for id {JokeId}", id);
            return cached;
        }

        var joke = await _jokeRepository.GetJokeByIdAsync(id);
        if (joke != null)
        {
            await _jokeCacheService.SetAsync(cacheKey, joke);
            _logger.LogInformation("Fetched joke from repository and cached for id {JokeId}", id);
        }
        else
        {
            _logger.LogWarning("Joke not found for id {JokeId}", id);
        }
        return joke;
    }

    public async Task<List<JokeDto>> GetJokesByIdsAsync(List<int> ids)
    {

        _logger.LogInformation("Fetching jokes by ids: {Ids}", string.Join(",", ids));
        return await _jokeRepository.GetJokesByIdsAsync(ids);
    }

    public async Task<bool> IsJokeOwnerAsync(int id, string userId)
    {
        var isOwner = await _jokeRepository.IsJokeOwnerAsync(id, userId);
        _logger.LogInformation("Checked joke ownership for joke {JokeId} and user {UserId}: {IsOwner}", id, userId, isOwner);
        return isOwner;
    }

    public async Task<JokeDto> GetRandomJokeAsync(List<int> viewedJokes)
    {

        _logger.LogInformation("Fetching random joke (excluding viewed: {ViewedJokes})", string.Join(",", viewedJokes));
        return await _jokeRepository.GetRandomJokeAsync(viewedJokes);
    }

    public async Task<JokeDto> GetRandomJokeAsync(List<int> viewedJokes, RandomJokeQueryObject query)
    {

        _logger.LogInformation("Fetching random joke with query (excluding viewed: {ViewedJokes})", string.Join(",", viewedJokes));
        return await _jokeRepository.GetRandomJokeAsync(viewedJokes, query);
    }

    public async Task<List<JokePreviewDto>> GetSuggestedByMeJokes(string userId)
    {
        _logger.LogInformation("Fetching suggested jokes by user {UserId}", userId);
        return await _jokeRepository.GetSuggestedByMeJokes(userId);
    }

    public async Task<bool> JokeExistsAsync(int id)
    {
        var exists = await _jokeRepository.JokeExistsAsync(id);
        _logger.LogInformation("Checked existence for joke {JokeId}: {Exists}", id, exists);
        return exists;
    }

    public async Task<SuggestedJokeDto> SuggestJokeAsync(JokeCreateDto? jokeCreateDto, string userId)
    {
        var joke = await _jokeRepository.CreateJokeAsync(jokeCreateDto, userId);
        await _jokeCacheService.InvalidateJokeAsync(joke.JokeId);
        _logger.LogInformation("Suggested joke {JokeId} by user {UserId} and invalidated cache.", joke.JokeId, userId);
        return new SuggestedJokeDto
        {
            JokeId = joke.JokeId,
            Text = joke.Text,
            ClassificationName = joke.Classification?.Name ?? "Unknown",
            IsApproved = joke.IsApproved
        };
    }

    public async Task<Joke> UpdateJokeAsync(int id, JokeUpdateDto? jokeUpdateDto)
    {
        var joke = await _jokeRepository.UpdateJokeAsync(id, jokeUpdateDto);
        await _jokeCacheService.InvalidateJokeAsync(id);
        _logger.LogInformation("Updated joke {JokeId} and invalidated cache.", id);
        return joke;
    }

    public async Task<Joke> UpdateJokeByUserAsync(int id, JokeUpdateDto? jokeUpdateDto)
    {
        var joke = await _jokeRepository.UpdateJokeAsync(id, jokeUpdateDto);
        await _jokeCacheService.InvalidateJokeAsync(id);
        _logger.LogInformation("Updated joke by user for joke {JokeId} and invalidated cache.", id);
        return joke;
    }
}
