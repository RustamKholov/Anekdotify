using System;
using frontend.DTOs;
using frontend.Jokes.DTOs;
using frontend.Models;

namespace frontend.Mappers;

public static class JokeMappers
{

    public static JokeCreateDTO ToJokeCreateDTOFromJoke(this Joke joke)
    {
        return new JokeCreateDTO()
        {
            Text = joke.Text,
            SourceId = joke.SourceId,
            ClassificationId = joke.ClassificationId,
        };
    }
}
