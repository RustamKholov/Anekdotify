using System;
using Anekdotify.BL.Helpers;
using Anekdotify.BL.Interfaces;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.Entities;

namespace Anekdotify.BL.Services;

public class JokeService(IJokeRepository jokeRepository) : IJokeService
{
    public async Task<Joke> CreateJokeAsync(JokeCreateDTO jokeCreateDTO, string userId)
    {
        return await jokeRepository.CreateJokeAsync(jokeCreateDTO, userId);
    }

    public async Task<Joke> DeleteJokeAsync(int id)
    {
        return await jokeRepository.DeleteJokeAsync(id);
    }

    public async Task<List<JokeDTO>> GetAllJokesAsync(JokesQueryObject query)
    {
        return await jokeRepository.GetAllJokesAsync(query);
    }

    public async Task<List<Comment>> GetCommentsByJokeIdAsync(int jokeId)
    {
        return await jokeRepository.GetCommentsByJokeIdAsync(jokeId);
    }

    public async Task<JokeDTO?> GetJokeByIdAsync(int id)
    {
        return await  jokeRepository.GetJokeByIdAsync(id);
    }

    public async Task<bool> JokeExistsAsync(int id)
    {
        return  await jokeRepository.JokeExistsAsync(id);
    }

    public async Task<Joke> UpdateJokeAsync(int id, JokeUpdateDTO jokeUpdateDTO)
    {
        return await jokeRepository.UpdateJokeAsync(id, jokeUpdateDTO);
    }
}
