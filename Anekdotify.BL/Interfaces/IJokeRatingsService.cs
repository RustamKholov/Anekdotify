using System;
using Anekdotify.Common;
using Anekdotify.Models.DTOs.JokeRating;

namespace Anekdotify.BL.Interfaces;

public interface IJokeRatingsService
{
    Task<OperationResult<RatingDTO>> SetJokeRatingAsync(JokeRatingDTO jokeRatingDTO, string userId);
    Task<OperationResult> RemoveJokeRatingAsync(int jokeId, string userId);
    Task<OperationResult<RatingDTO>> GetJokeRatingByUserAsync(int jokeId, string userId);
}
