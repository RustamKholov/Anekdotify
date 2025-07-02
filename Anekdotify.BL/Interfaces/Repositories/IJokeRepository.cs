using Anekdotify.BL.Helpers;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.Entities;

namespace Anekdotify.BL.Interfaces.Repositories
{
    public interface IJokeRepository
    {
        Task<List<JokeDto>> GetAllJokesAsync(JokesQueryObject query);
        Task<List<JokeDto>> GetJokesByIdsAsync(List<int> ids);
        Task<JokeDto?> GetJokeByIdAsync(int id);
        Task<Joke> CreateJokeAsync(JokeCreateDto? jokeCreateDto, string userId);
        Task<Joke> UpdateJokeAsync(int id, JokeUpdateDto? jokeUpdateDto);
        Task<Joke> UpdateJokeByUserAsync(int id, JokeUpdateDto? jokeUpdateDto);
        Task<List<JokePreviewDto>> GetSuggestedByMeJokes(string userId);
        Task<Joke> DeleteJokeAsync(int id);
        Task<List<Comment>> GetCommentsByJokeIdAsync(int jokeId);
        Task<bool> JokeExistsAsync(int id);
        Task<bool> IsJokeOwnerAsync(int id, string userId);
        Task<JokeDto> GetRandomJokeAsync(List<int> viewedJokes);
        Task<JokeDto> GetRandomJokeAsync(List<int> viewedJokes, RandomJokeQueryObject query);
    }
}