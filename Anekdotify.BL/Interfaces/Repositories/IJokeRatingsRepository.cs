using Anekdotify.Common;
using Anekdotify.Models.DTOs.JokeRating;

namespace Anekdotify.BL.Interfaces.Repositories
{
    public interface IJokeRatingsRepository
    {
        Task<OperationResult<RatingDto>> SetJokeRatingAsync(JokeRatingDto jokeRatingDto, string userId);
        Task<OperationResult> RemoveJokeRatingAsync(int jokeId, string userId);
        Task<OperationResult<RatingDto>> GetJokeRatingByUserAsync(int jokeId, string userId);

    }
}