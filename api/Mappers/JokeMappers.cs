using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTOs.Jokes;
using api.Models;

namespace api.Mappers
{
    public static class JokeMappers
    {
        public static JokeDTO ToJokeDTO(this Joke joke)
        {
            if (joke == null)
            {
                throw new ArgumentNullException(nameof(joke), "Joke cannot be null.");
            }

            return new JokeDTO
            {
                Id = joke.Id,
                Title = joke.Title,
                Content = joke.Content,
                Comments = joke.Comments.Select(c => c.ToCommentDTO()).ToList()
            };
        }
        public static Joke ToJokeFromCreateDTO(this JokeCreateDTO jokeCreateDTO)
        {
            if (jokeCreateDTO == null)
            {
                throw new ArgumentNullException(nameof(jokeCreateDTO), "JokeCreateDTO cannot be null.");
            }

            return new Joke
            {
                Title = jokeCreateDTO.Title,
                Content = jokeCreateDTO.Content
            };
        }
        public static Joke UpdateJokeFromJokeDTO(this Joke jokeModel, JokeUpdateDTO jokeUpdateDTO)
        {
            if (!string.IsNullOrWhiteSpace(jokeUpdateDTO.Title))
            {
                jokeModel.Title = jokeUpdateDTO.Title;
            }
            if (!string.IsNullOrWhiteSpace(jokeUpdateDTO.Content))
            {
                jokeModel.Content = jokeUpdateDTO.Content;
            }
            return jokeModel;
        }
    }
}