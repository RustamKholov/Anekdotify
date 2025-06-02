using System;
using frontend.DTOs;
using frontend.Models;

namespace frontend.Mappers;

public static class JokeMappers
{

    public static Joke ToJokeFromCreateDTO(this JokeCreateDTO jokeCreateDTO, int id)
    {
        return new Joke()
        {
            Id = id,
            Title = jokeCreateDTO.Title,
            Content = jokeCreateDTO.Content
        };
    }
}
