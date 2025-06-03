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
    public static JokeCreateDTO ToJokeCreateDTOFromJoke(this Joke joke)
    {
        return new JokeCreateDTO()
        {
            Title = joke.Title,
            Content = joke.Content
        };
    }
    public static Joke UpdateJokeFromEditDTO(this Joke joke, JokeEditDTO jokeEditDTO)
    {
        if (!string.IsNullOrEmpty(jokeEditDTO.Title))
        {
            joke.Title = jokeEditDTO.Title;
        }
        if (!string.IsNullOrEmpty(jokeEditDTO.Content))
        {
            joke.Content = jokeEditDTO.Content;
        }
        return joke;
    }
    public static JokeEditDTO ToEditFromCreate(this JokeCreateDTO jokeCreateDTO, int jokeId)
    {
        return new JokeEditDTO()
        {
            Id = jokeId,
            Title = jokeCreateDTO.Title,
            Content = jokeCreateDTO.Content
        };
    }
}
