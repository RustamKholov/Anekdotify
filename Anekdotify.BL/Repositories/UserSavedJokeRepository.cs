using Anekdotify.BL.Interfaces;
using Anekdotify.Common;
using Anekdotify.Database.Data;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.DTOs.SaveJoke;
using Anekdotify.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Anekdotify.BL.Repositories
{
    public class UserSavedJokeRepository : IUserSavedJokeRepository
    {
        private readonly ApplicationDBContext _context;

        public UserSavedJokeRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task<List<JokePreviewDTO>> GetSavedJokesForUserAsync(string userId)
        {
            return await _context.UserSavedJokes
                        .Where(usj => usj.UserId == userId)
                        .Select(usj => new JokePreviewDTO
                        {
                            JokeId = usj.Joke.JokeId,
                            Text = usj.Joke.Text,
                            ClassificationName = usj.Joke.Classification != null ? usj.Joke.Classification.Name : "Unknown",
                            CommentCount = usj.Joke.Comments.Count(),
                            LikeCount = usj.Joke.JokeRatings.Count(),
                            Source = usj.Joke.Source != null ? usj.Joke.Source.SourceName : "Unknown",
                            SubmissionDate = usj.Joke.SubbmissionDate
                        })
                        .OrderByDescending(dto => dto.SubmissionDate)
                        .ToListAsync();
        }

        public async Task<bool> IsJokeSavedByUserAsync(SaveJokeDTO saveJokeDTO, string userId)
        {
            return await _context.UserSavedJokes.AnyAsync(usj => usj.UserId == userId && usj.JokeId == saveJokeDTO.JokeId);
        }

        public async Task<OperationResult> RemoveSavedJokeAsync(SaveJokeDTO saveJokeDTO, string userId)
        {
            var entry = await _context.UserSavedJokes.FirstOrDefaultAsync(suj => suj.UserId == userId && suj.JokeId == saveJokeDTO.JokeId);
            if (entry == null)
            {
                return OperationResult.NotFound("Joke does not saved by user");
            }

            _context.UserSavedJokes.Remove(entry);
            await _context.SaveChangesAsync();
            return OperationResult.Success();
        }

        public async Task<OperationResult> SaveJokeAsync(SaveJokeDTO saveJokeDTO, string userId)
        {
            var jokeExists = await _context.Jokes.AnyAsync(j => j.JokeId == saveJokeDTO.JokeId);
            if (!jokeExists)
            {
                return OperationResult.NotFound("Joke not found");
            }
            var existingSavedJoke = await _context.UserSavedJokes.FirstOrDefaultAsync(usj => usj.JokeId == saveJokeDTO.JokeId && usj.UserId == userId);

            if (existingSavedJoke != null)
            {
                return OperationResult.AlreadyExists("Joke is already saved by this user");
            }

            var userSavedJoke = new UserSavedJoke
            {
                JokeId = saveJokeDTO.JokeId,
                UserId = userId
            };

            await _context.UserSavedJokes.AddAsync(userSavedJoke);
            await _context.SaveChangesAsync();

            return OperationResult.Success();
        }
    }
}