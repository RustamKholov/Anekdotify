using Anekdotify.BL.Helpers;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.Entities;

namespace Anekdotify.BL.Interfaces.Services;

public interface IJokeService
{
    Task<List<JokeDTO>> GetAllJokesAsync(JokesQueryObject query);
    Task<List<JokeDTO>> GetJokesByIdsAsync(List<int> ids);
    Task<JokeDTO?> GetJokeByIdAsync(int id);
    Task<Joke> CreateJokeAsync(JokeCreateDTO jokeCreateDTO, string userId);
    Task<SuggestedJokeDTO> SuggestJokeAsync(JokeCreateDTO jokeCreateDTO, string userId);
    Task<List<JokeDTO>> GetSuggestedByMeJokes(string userId);
    Task<Joke> UpdateJokeAsync(int id, JokeUpdateDTO jokeUpdateDTO);
    Task<Joke> DeleteJokeAsync(int id);
    Task<List<Comment>> GetCommentsByJokeIdAsync(int jokeId);
    Task<bool> JokeExistsAsync(int id);
    Task<JokeDTO> GetRandomJokeAsync(List<int> viewedJokes);
}
