using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTOs.Comments;
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
                JokeId = joke.JokeId,
                Text = joke.Text,
                SubmissionDate = joke.SubbmissionDate,
                SourceId = joke.SourceId,
                SourceName = joke.Source?.SourceName,
                ClassificationId = joke.ClassificationId,
                ClassificationName = joke.Classification?.Name, 
                TotalLikes = joke.JokeRatings.Count(jr => jr.Rating),
                TotalDislikes = joke.JokeRatings.Count(jr => !jr.Rating ),
                Comments = joke.Comments.ToList().BuildHierarchicalComments()
            };
        }
        public static Joke ToJokeFromCreateDTO(this JokeCreateDTO jokeCreateDTO, string userId)
        {
            if (jokeCreateDTO == null)
            {
                throw new ArgumentNullException(nameof(jokeCreateDTO), "JokeCreateDTO cannot be null.");
            }

            return new Joke
            {
                Text = jokeCreateDTO.Text,
                ClassificationId = jokeCreateDTO.ClassificationId,
                SourceId = jokeCreateDTO.SourceId ?? 0,
                SubbmitedByUserId = userId
            };
        }
        public static Joke UpdateJokeFromJokeDTO(this Joke jokeModel, JokeUpdateDTO jokeUpdateDTO)
        {
            if (!string.IsNullOrWhiteSpace(jokeUpdateDTO.Text))
            {
                jokeModel.Text = jokeUpdateDTO.Text;
            }
            if (jokeUpdateDTO.ClassificationId != null)
            {
                jokeModel.ClassificationId = jokeUpdateDTO.ClassificationId;
            }
            return jokeModel;
        }

        
    }
}