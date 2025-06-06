using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTOs.Jokes;
using api.Helpers;
using api.Models;

namespace api.Interfaces
{
    public interface IJokeRepository
    {
        Task<List<JokeDTO>> GetAllJokesAsync(JokesQueryObject query);
        Task<JokeDTO?> GetJokeByIdAsync(int id);
        Task<Joke> CreateJokeAsync(JokeCreateDTO jokeCreateDTO);
        Task<Joke> UpdateJokeAsync(int id, JokeUpdateDTO jokeUpdateDTO);
        Task<Joke> DeleteJokeAsync(int id);
        Task<List<Comment>> GetCommentsByJokeIdAsync(int jokeId);
        Task<bool> JokeExists(int id);

    }
}