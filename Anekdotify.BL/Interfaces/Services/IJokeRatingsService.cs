using Anekdotify.Common;
using Anekdotify.Models.DTOs.JokeRating;

namespace Anekdotify.BL.Interfaces.Services;

public interface IJokeRatingsService
{
    Task<OperationResult<RatingDto>> SetJokeRatingAsync(JokeRatingDto jokeRatingDto, string userId);
    Task<OperationResult> RemoveJokeRatingAsync(int jokeId, string userId);
    Task<OperationResult<RatingDto>> GetJokeRatingByUserAsync(int jokeId, string userId);
}
