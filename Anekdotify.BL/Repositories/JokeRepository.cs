using Anekdotify.BL.Helpers;
using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Mappers;
using Anekdotify.Database.Data;
using Anekdotify.Models.DTOs.Comments;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.Entities;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

            var baseJokes = await _context.Jokes
                .AsNoTracking()
                .Where(j => j.SourceId != -4)
                .OrderByDescending(j => j.SubbmissionDate)
                .Skip(skip)
                .Take(query.PageSize)
                .Select(j => new
                {
                    j.JokeId,
                    j.Text,
                    ClassificationName = j.Classification.Name,
                    j.ClassificationId,
                    SourceName = j.Source.SourceName,
                    j.SourceId
                })
                .ToListAsync();

            var jokeIds = baseJokes.Select(j => j.JokeId).ToList();

            var ratings = await _context.JokeRatings
                .AsNoTracking()
                .Where(r => jokeIds.Contains(r.JokeId))
                .GroupBy(r => r.JokeId)
                .Select(g => new
                {
                    JokeId = g.Key,
                    Likes = g.Count(r => r.Rating),
                    Dislikes = g.Count(r => !r.Rating)
                })
                .ToListAsync();

            var comments = await _context.Comments
                .AsNoTracking()
                .Where(c => jokeIds.Contains(c.JokeId))
                .Select(c => new
                {
                    c.JokeId,
                    c.CommentText,
                    Username = c.User.UserName,
                    CommentId = c.CommentId,
                    Ratings = c.CommentRatings
                })
                .ToListAsync();

            var commentRatings = comments.Select(c => new CommentDTO
            {
                CommentText = c.CommentText,
                Username = c.Username ?? "Unknown",
                TotalLikes = c.Ratings.Count(r => r.Rating),
                TotalDislikes = c.Ratings.Count(r => !r.Rating)
            });

            var groupedComments = comments
                .GroupBy(c => c.JokeId)
                .ToDictionary(g => g.Key, g => g.Select(c => new CommentDTO
                {
                    CommentText = c.CommentText,
                    Username = c.Username ?? "Unknown",
                    TotalLikes = c.Ratings.Count(r => r.Rating),
                    TotalDislikes = c.Ratings.Count(r => !r.Rating)
                }).ToList());

            var jokeDTOs = baseJokes.Select(j => new JokeDTO
            {
                JokeId = j.JokeId,
                Text = j.Text,
                ClassificationName = j.ClassificationName ?? "Unknown",
                ClassificationId = j.ClassificationId,
                SourceName = j.SourceName ?? "Unknown",
                SourceId = j.SourceId,
                TotalLikes = ratings.FirstOrDefault(r => r.JokeId == j.JokeId)?.Likes ?? 0,
                TotalDislikes = ratings.FirstOrDefault(r => r.JokeId == j.JokeId)?.Dislikes ?? 0,
                Comments = groupedComments.TryGetValue(j.JokeId, out var cmts) ? cmts : new List<CommentDTO>()
            }).ToList();

            return jokeDTOs;
        }
        public async Task<JokeDTO?> GetJokeByIdAsync(int jokeId)
        {
            var baseJoke = await _context.Jokes
                .AsNoTracking()
                .Where(j => j.JokeId == jokeId)
                .Select(j => new
                {
                    j.JokeId,
                    j.Text,
                    ClassificationName = j.Classification.Name,
                    j.ClassificationId,
                    SourceName = j.Source.SourceName,
                    j.SourceId
                })
                .FirstOrDefaultAsync();

            if (baseJoke == null)
            {
                return null;
            }
            var ratings = await _context.JokeRatings
                 .AsNoTracking()
                 .Where(r => r.JokeId == baseJoke.JokeId)
                 .GroupBy(r => r.JokeId)
                 .Select(g => new
                 {
                     JokeId = g.Key,
                     Likes = g.Count(r => r.Rating),
                     Dislikes = g.Count(r => !r.Rating)
                 })
                 .ToListAsync();

            var comments = await _context.Comments
                .AsNoTracking()
                .Where(c => c.JokeId == baseJoke.JokeId)
                .Select(c => new
                {
                    c.JokeId,
                    c.CommentText,
                    Username = c.User.UserName,
                    CommentId = c.CommentId,
                    Ratings = c.CommentRatings
                })
                .ToListAsync();

            var commentRatings = comments.Select(c => new CommentDTO
            {
                CommentText = c.CommentText,
                Username = c.Username ?? "Unknown",
                TotalLikes = c.Ratings.Count(r => r.Rating),
                TotalDislikes = c.Ratings.Count(r => !r.Rating)
            });

            var groupedComments = comments
                .GroupBy(c => c.JokeId)
                .ToDictionary(g => g.Key, g => g.Select(c => new CommentDTO
                {
                    CommentText = c.CommentText,
                    Username = c.Username ?? "Unknown",
                    TotalLikes = c.Ratings.Count(r => r.Rating),
                    TotalDislikes = c.Ratings.Count(r => !r.Rating)
                }).ToList());

            var jokeDTO = new JokeDTO
            {
                JokeId = baseJoke.JokeId,
                Text = baseJoke.Text,
                ClassificationName = baseJoke.ClassificationName ?? "Unknown",
                ClassificationId = baseJoke.ClassificationId,
                SourceName = baseJoke.SourceName ?? "Unknown",
                SourceId = baseJoke.SourceId,
                TotalLikes = ratings.FirstOrDefault(r => r.JokeId == baseJoke.JokeId)?.Likes ?? 0,
                TotalDislikes = ratings.FirstOrDefault(r => r.JokeId == baseJoke.JokeId)?.Dislikes ?? 0,
                Comments = groupedComments.TryGetValue(baseJoke.JokeId, out var cmts) ? cmts : new List<CommentDTO>()
            };

            return jokeDTO;
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
            var baseJokes = await _context.Jokes
                .AsNoTracking()
                .Where(j => ids.Contains(j.JokeId))
                .OrderByDescending(j => j.SubbmissionDate)
                .Select(j => new
                {
                    j.JokeId,
                    j.Text,
                    ClassificationName = j.Classification.Name,
                    j.ClassificationId,
                    SourceName = j.Source.SourceName,
                    j.SourceId
                })
                .ToListAsync();

            var jokeIds = baseJokes.Select(j => j.JokeId).ToList();

            var ratings = await _context.JokeRatings
                .AsNoTracking()
                .Where(r => jokeIds.Contains(r.JokeId))
                .GroupBy(r => r.JokeId)
                .Select(g => new
                {
                    JokeId = g.Key,
                    Likes = g.Count(r => r.Rating),
                    Dislikes = g.Count(r => !r.Rating)
                })
                .ToListAsync();

            var comments = await _context.Comments
                .AsNoTracking()
                .Where(c => jokeIds.Contains(c.JokeId))
                .Select(c => new
                {
                    c.JokeId,
                    c.CommentText,
                    Username = c.User.UserName,
                    CommentId = c.CommentId,
                    Ratings = c.CommentRatings
                })
                .ToListAsync();

            var commentRatings = comments.Select(c => new CommentDTO
            {
                CommentText = c.CommentText,
                Username = c.Username ?? "Unknown",
                TotalLikes = c.Ratings.Count(r => r.Rating),
                TotalDislikes = c.Ratings.Count(r => !r.Rating)
            });

            var groupedComments = comments
                .GroupBy(c => c.JokeId)
                .ToDictionary(g => g.Key, g => g.Select(c => new CommentDTO
                {
                    CommentText = c.CommentText,
                    Username = c.Username ?? "Unknown",
                    TotalLikes = c.Ratings.Count(r => r.Rating),
                    TotalDislikes = c.Ratings.Count(r => !r.Rating)
                }).ToList());

            var jokeDTOs = baseJokes.Select(j => new JokeDTO
            {
                JokeId = j.JokeId,
                Text = j.Text,
                ClassificationName = j.ClassificationName ?? "Unknown",
                ClassificationId = j.ClassificationId,
                SourceName = j.SourceName ?? "Unknown",
                SourceId = j.SourceId,
                TotalLikes = ratings.FirstOrDefault(r => r.JokeId == j.JokeId)?.Likes ?? 0,
                TotalDislikes = ratings.FirstOrDefault(r => r.JokeId == j.JokeId)?.Dislikes ?? 0,
                Comments = groupedComments.TryGetValue(j.JokeId, out var cmts) ? cmts : new List<CommentDTO>()
            }).ToList();

            return jokeDTOs;
        }
    }
}