using Anekdotify.BL.Helpers;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.Entities;

namespace Anekdotify.BL.Interfaces.Services;

public interface IJokeService
{
    Task<List<JokeDto>> GetAllJokesAsync(JokesQueryObject query);
    Task<List<JokeDto>> GetJokesByIdsAsync(List<int> ids);
    Task<JokeDto?> GetJokeByIdAsync(int id);
    Task<Joke> CreateJokeAsync(JokeCreateDto? jokeCreateDto, string userId);
    Task<SuggestedJokeDto> SuggestJokeAsync(JokeCreateDto? jokeCreateDto, string userId);
    Task<List<JokePreviewDto>> GetSuggestedByMeJokes(string userId);
    Task<Joke> UpdateJokeAsync(int id, JokeUpdateDto? jokeUpdateDto);
    Task<Joke> DeleteJokeAsync(int id);
    Task<List<Comment>> GetCommentsByJokeIdAsync(int jokeId);
    Task<bool> JokeExistsAsync(int id);
    Task<JokeDto> GetRandomJokeAsync(List<int> viewedJokes);
}
