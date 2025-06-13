using Anekdotify.BL.Helpers;
using Anekdotify.BL.Interfaces;
using Anekdotify.BL.Mappers;
using Anekdotify.Database.Data;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Anekdotify.BL.Repositories
{
    public class JokeRepository : IJokeRepository
    {
        private readonly ApplicationDBContext _context;
        public JokeRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task<List<JokeDTO>> GetAllJokesAsync(JokesQueryObject query)
        {
            var jokes = _context.Jokes
                .Include(j => j.Classification) // To get ClassificationName
                .Include(j => j.JokeRatings)     // To calculate TotalLikes/Dislikes
                .Include(j => j.Comments)        // To get comments
                    .ThenInclude(c => c.User)    // To get Usernames for comments
                    .ThenInclude(c => c.CommentRatings)
                .AsQueryable()
                .AsSplitQuery();
            if (query.AddingDay.HasValue)
            {
                var day = query.AddingDay.Value.Date;
                var nextDay = day.AddDays(1);
                jokes = jokes.Where(j => j.SubbmissionDate >= day && j.SubbmissionDate < nextDay);
            }
            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                if (string.Equals(query.SortBy, "AddingDay", StringComparison.OrdinalIgnoreCase))
                {
                    jokes = query.ByDescending ? jokes.OrderByDescending(j => j.SubbmissionDate) : jokes.OrderBy(j => j.SubbmissionDate);
                }
            }
            var skipNumber = (query.PageNumber - 1) * query.PageSize;

            var allJokes = await jokes.Skip(skipNumber).Take(query.PageSize).ToListAsync();
            var allJokesDTOS = allJokes.Select(j => j.ToJokeDTO()).ToList();
            return allJokesDTOS;
        }
        public async Task<JokeDTO?> GetJokeByIdAsync(int jokeId)
        {
            var joke = await _context.Jokes
            .Where(j => j.JokeId == jokeId) 
            .Include(j => j.Source)
            .Include(j => j.Classification) // To get ClassificationName
            .Include(j => j.JokeRatings)     // To calculate TotalLikes/Dislikes
            .Include(j => j.Comments)        // To get comments
                .ThenInclude(c => c.User)    // To get Usernames for comments
                .ThenInclude(c => c.CommentRatings) // To get ratings for comments
            .AsSplitQuery()
            .FirstOrDefaultAsync();
            if (joke == null)
            {
                return null;
            }
            var jokeDTO = joke.ToJokeDTO();
            return jokeDTO;
        }
        public async Task<Joke> CreateJokeAsync(JokeCreateDTO jokeCreateDTO, string userId)
        {
            if (jokeCreateDTO == null || string.IsNullOrWhiteSpace(jokeCreateDTO.Text))
            {
                throw new ArgumentException("Joke content cannot be empty.");
            }

            var joke = jokeCreateDTO.ToJokeFromCreateDTO(userId);

            await _context.Jokes.AddAsync(joke);
            await _context.SaveChangesAsync();

            return joke;
        }
        public async Task<Joke> UpdateJokeAsync(int id, JokeUpdateDTO jokeUpdateDTO)
        {
            var joke = await _context.Jokes.FindAsync(id);
            if (joke == null)
            {
                throw new KeyNotFoundException($"Joke with ID {id} not found.");
            }
            if (jokeUpdateDTO == null)
            {
                throw new ArgumentException("Joke update cannot be empty.");
            }
            joke = joke.UpdateJokeFromJokeDTO(jokeUpdateDTO);
            _context.Jokes.Update(joke);
            await _context.SaveChangesAsync();
            return joke;
        }
        public async Task<Joke> DeleteJokeAsync(int id)
        {
            var joke = await _context.Jokes.FindAsync(id);
            if (joke == null)
            {
                throw new KeyNotFoundException($"Joke with ID {id} not found.");
            }
            _context.Jokes.Remove(joke);
            var commentsToDelete = _context.Comments.Where(c => c.JokeId == joke.JokeId);
            _context.Comments.RemoveRange(commentsToDelete);
            await _context.SaveChangesAsync();
            return joke;
        }
        public async Task<List<Comment>> GetCommentsByJokeIdAsync(int jokeId)
        {
            return await _context.Comments
                .Where(c => c.JokeId == jokeId)
                .ToListAsync();
        }

        public async Task<bool> JokeExistsAsync(int jokeId)
        {
            return await _context.Jokes.AnyAsync(j => j.JokeId == jokeId);
        }

        public async Task<JokeDTO> GetRandomJokeAsync(List<int> viewedJokes)
        {
            
            var jokeIds = await _context.Jokes.Select(j => j.JokeId).ToListAsync();

            if (jokeIds == null || jokeIds.Count == 0)
            {
                 throw new InvalidOperationException("No jokes available to select from.");
            }

            var random = new Random();
            var randomIndex = random.Next(0, jokeIds.Count);
            if(viewedJokes != null && viewedJokes.Count > 0)
            {
                // Filter out viewed jokes
                jokeIds = jokeIds.Where(id => !viewedJokes.Contains(id)).ToList();
                if (jokeIds.Count == 0)
                {
                    throw new InvalidOperationException("No unviewed jokes available to select from.");
                }
                randomIndex = random.Next(0, jokeIds.Count);
            }
            var randomJokeId = jokeIds[randomIndex];

            
            return await GetJokeByIdAsync(randomJokeId) ?? throw new KeyNotFoundException($"Joke with ID {randomJokeId} not found.");
        }
    }
}