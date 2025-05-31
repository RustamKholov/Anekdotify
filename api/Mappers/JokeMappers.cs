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
                Content = jokeCreateDTO.Content
            };
        }
      
    }
}