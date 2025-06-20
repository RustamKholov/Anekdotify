using Anekdotify.BL.Helpers;
using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Mappers;
using Anekdotify.Database.Data;
using Anekdotify.Models.DTOs.Comments;
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
            var skip = (query.PageNumber - 1) * query.PageSize;

            var jokes = await _context.Jokes
                .Where(j => j.SourceId != -4) // Exclude suggested jokes (id -4)
                .OrderByDescending(j => j.SubbmissionDate)
                .Skip(skip)
                .Take(query.PageSize)
                .Select(j => new JokeDTO
                {
                    JokeId = j.JokeId,
                    Text = j.Text,
                    ClassificationName = j.Classification.Name ?? "Unknown",
                    ClassificationId = j.ClassificationId,
                    SourceName = j.Source.SourceName ?? "Unknown",
                    SourceId = j.SourceId,
                    TotalLikes = j.JokeRatings.Count(r => r.Rating),
                    TotalDislikes = j.JokeRatings.Count(r => !r.Rating),
                    Comments = j.Comments.Select(c => new CommentDTO
                    {
                        CommentText = c.CommentText,
                        Username = c.User.UserName ?? "Unknown",
                        TotalLikes = c.CommentRatings.Count(r => r.Rating),
                        TotalDislikes = c.CommentRatings.Count(r => !r.Rating)
                    }).ToList()
                })
                .ToListAsync();

            return jokes;
        }
        public async Task<JokeDTO?> GetJokeByIdAsync(int jokeId)
        {
            var joke = await _context.Jokes
            .Where(j => j.JokeId == jokeId)
            .Select(j => new JokeDTO
            {
                JokeId = j.JokeId,
                Text = j.Text,
                ClassificationName = j.Classification.Name ?? "Unknown",
                ClassificationId = j.ClassificationId,
                SourceName = j.Source.SourceName ?? "Unknown",
                SourceId = j.SourceId,
                TotalLikes = j.JokeRatings.Count(r => r.Rating),
                TotalDislikes = j.JokeRatings.Count(r => !r.Rating),
                Comments = j.Comments.Select(c => new CommentDTO
                {
                    CommentText = c.CommentText,
                    Username = c.User.UserName ?? "Unknown",
                    TotalLikes = c.CommentRatings.Count(r => r.Rating),
                    TotalDislikes = c.CommentRatings.Count(r => !r.Rating)
                }).ToList()
            })
            .FirstOrDefaultAsync();

            if (joke == null)
            {
                return null;
            }
           
            return joke;
        }
        public async Task<Joke> CreateJokeAsync(JokeCreateDTO jokeCreateDTO, string userId)
        {
            if (jokeCreateDTO == null || string.IsNullOrWhiteSpace(jokeCreateDTO.Text))
            {
                throw new ArgumentException("Joke content cannot be empty.");
            }

            var joke = jokeCreateDTO.ToJokeFromCreateDTO(userId);

            var res = await _context.Jokes.AddAsync(joke);
            await _context.SaveChangesAsync();

            return res.Entity;
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
            
            var jokeIds = await _context.Jokes.Where(j => j.SourceId != -4).Select(j => j.JokeId).ToListAsync(); 

            if (jokeIds == null || jokeIds.Count == 0)
            {
                 throw new InvalidOperationException("No jokes available to select from.");
            }

            var random = new Random();
            var randomIndex = random.Next(0, jokeIds.Count);
            if(viewedJokes != null && viewedJokes.Count > 0)
            {
                
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

        public async Task<List<JokeDTO>> GetJokesByIdsAsync(List<int> ids)
        {
            var jokes = await _context.Jokes
                .Where(j => ids.Contains(j.JokeId))
                .Include(j => j.Source)
                .Include(j => j.Classification)
                .Include(j => j.JokeRatings)
                .Include(j => j.Comments)
                    .ThenInclude(c => c.User)
                    .ThenInclude(u => u.CommentRatings)
                .AsSplitQuery()
                .ToListAsync();

            return jokes.Select(j => j.ToJokeDTO()).ToList();
        }
    }
}