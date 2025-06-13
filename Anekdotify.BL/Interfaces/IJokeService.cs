using System;
using Anekdotify.BL.Helpers;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.Entities;

namespace Anekdotify.BL.Interfaces;

public interface IJokeService
{
    Task<List<JokeDTO>> GetAllJokesAsync(JokesQueryObject query);
    Task<JokeDTO?> GetJokeByIdAsync(int id);
    Task<Joke> CreateJokeAsync(JokeCreateDTO jokeCreateDTO, string userId);
    Task<Joke> UpdateJokeAsync(int id, JokeUpdateDTO jokeUpdateDTO);
    Task<Joke> DeleteJokeAsync(int id);
    Task<List<Comment>> GetCommentsByJokeIdAsync(int jokeId);
    Task<bool> JokeExistsAsync(int id);
    Task<JokeDTO> GetRandomJokeAsync(List<int> viewedJokes);
}
