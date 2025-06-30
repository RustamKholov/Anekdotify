using Anekdotify.Models.DTOs.Comments;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.Entities;

namespace Anekdotify.BL.Mappers
{
    public static class JokeMappers
    {
        public static JokeDto ToJokeDto(this Joke joke)
        {
            if (joke == null)
            {
                throw new ArgumentNullException(nameof(joke), "Joke cannot be null.");
            }

            return new JokeDto
            {
                JokeId = joke.JokeId,
                Text = joke.Text,
                SubmissionDate = joke.SubbmissionDate,
                SourceId = joke.SourceId,
                SourceName = joke.Source?.SourceName,
                ClassificationId = joke.ClassificationId,
                ClassificationName = joke.Classification?.Name,
                TotalLikes = joke.JokeRatings.Count(jr => jr.Rating),
                TotalDislikes = joke.JokeRatings.Count(jr => !jr.Rating),
                Comments = joke.Comments.Select(c => c.ToCommentDto()).ToList().BuildHierarchicalComments()
            };
        }
        public static Joke ToJokeFromCreateDto(this JokeCreateDto? jokeCreateDto, string userId)
        {
            if (jokeCreateDto == null)
            {
                throw new ArgumentNullException(nameof(jokeCreateDto), "JokeCreateDTO cannot be null.");
            }

            return new Joke
            {
                Text = jokeCreateDto.Text,
                ClassificationId = jokeCreateDto.ClassificationId,
                SourceId = jokeCreateDto.SourceId ?? 0,
                SubbmitedByUserId = userId
            };
        }
        public static Joke UpdateJokeFromJokeDto(this Joke jokeModel, JokeUpdateDto? jokeUpdateDto)
        {
            if (!string.IsNullOrWhiteSpace(jokeUpdateDto?.Text))
            {
                jokeModel.Text = jokeUpdateDto.Text;
            }
            if (jokeUpdateDto?.ClassificationId != null)
            {
                jokeModel.ClassificationId = jokeUpdateDto.ClassificationId;
            }
            return jokeModel;
        }

        public static JokeDto ToDtoFromPreviewDto(this JokePreviewDto jokePreview)
        {
            if (jokePreview == null)
            {
                throw new ArgumentNullException(nameof(jokePreview), "JokePreviewDTO cannot be null.");
            }
            return new JokeDto
            {
                JokeId = jokePreview.JokeId,
                Text = jokePreview.Text,
                SubmissionDate = jokePreview.SubmissionDate,
                ClassificationName = jokePreview.ClassificationName,
                TotalLikes = jokePreview.LikeCount,
                TotalDislikes = jokePreview.DislikeCount,
                SourceName = jokePreview.Source,
                Comments = new List<CommentDto>(jokePreview.CommentCount)
            };
        }
        public static JokePreviewDto ToPreviewDtoFromDto(this JokeDto jokeDto)
        {
            return new JokePreviewDto
            {
                JokeId = jokeDto.JokeId,
                Text = jokeDto.Text,
                SubmissionDate = jokeDto.SubmissionDate,
                ClassificationName = jokeDto.ClassificationName ?? "Unknown", 
                LikeCount  = jokeDto.TotalLikes,
                DislikeCount = jokeDto.TotalDislikes,
                CommentCount = jokeDto.Comments.Count,
                Source = jokeDto.SourceName ?? "Unknown"
            };
        }
        public static JokePreviewDto ToPreviewFromJoke (this Joke joke)
        {
            return new JokePreviewDto
            {
                JokeId = joke.JokeId,
                Text = joke.Text,
                ClassificationName = joke.Classification?.Name ?? "Unknown",
                CommentCount = joke.Comments.Count,
                LikeCount = joke.JokeRatings.Count,
                Source = joke.Source?.SourceName ?? "Unknown",
                SubmissionDate = joke.SubbmissionDate
            };
        }
    }
}