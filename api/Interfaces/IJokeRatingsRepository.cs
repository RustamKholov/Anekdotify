using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Common;
using api.DTOs.JokeRating;
using Azure;

namespace api.Interfaces
{
    public interface IJokeRatingsRepository
    {
        Task<OperationResult<RatingDTO>> SetJokeRatingAsync(JokeRatingDTO jokeRatingDTO, string userId);
        Task<OperationResult> RemoveJokeRatingAsync(int jokeId, string userId);
        Task<OperationResult<RatingDTO>> GetJokeRatingByUserAsync(int jokeId, string userId);

    }
}