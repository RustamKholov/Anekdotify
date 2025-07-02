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
        public async Task<List<JokeDto>> GetAllJokesAsync(JokesQueryObject query)
        {
            var skip = (query.PageNumber - 1) * query.PageSize;

            var baseJokes = await _context.Jokes
                .AsNoTracking()
                .Where(j => j.IsApproved == true)
                .OrderByDescending(j => j.SubbmissionDate)
                .Skip(skip)
                .Take(query.PageSize)
                .Select(j => new
                {
                    j.JokeId,
                    j.Text,
                    ClassificationName = j.Classification != null ? j.Classification.Name : null,
                    j.ClassificationId,
                    SourceName = j.Source != null ? j.Source.SourceName : null,
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
                    c.CommentId,
                    Ratings = c.CommentRatings,
                    c.CommentDate,
                    c.ParentCommentId
                })
                .ToListAsync();


            var groupedCommentsDict = comments
                    .GroupBy(c => c.JokeId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(c => new CommentDto
                        {
                            JokeId = c.JokeId,
                            CommentId = c.CommentId,
                            ParentCommentId = c.ParentCommentId,
                            CommentDate = c.CommentDate,
                            CommentText = c.CommentText,
                            Username = c.Username ?? "Unknown",
                            TotalLikes = c.Ratings.Count(r => r.Rating),
                            TotalDislikes = c.Ratings.Count(r => !r.Rating)
                        }).ToList()
                    );

            var jokeDtOs = baseJokes.Select(j => new JokeDto
            {
                JokeId = j.JokeId,
                Text = j.Text,
                ClassificationName = j.ClassificationName ?? "Unknown",
                ClassificationId = j.ClassificationId,
                SourceName = j.SourceName ?? "Unknown",
                SourceId = j.SourceId,
                TotalLikes = ratings.FirstOrDefault(r => r.JokeId == j.JokeId)?.Likes ?? 0,
                TotalDislikes = ratings.FirstOrDefault(r => r.JokeId == j.JokeId)?.Dislikes ?? 0,
                Comments = groupedCommentsDict.TryGetValue(j.JokeId, out List<CommentDto>? jokeComments)
                    ? jokeComments.BuildHierarchicalComments()
                    : new List<CommentDto>()
            }).ToList();

            return jokeDtOs;
        }
        public async Task<JokeDto?> GetJokeByIdAsync(int jokeId)
        {
            var baseJoke = await _context.Jokes
                .AsNoTracking()
                .Where(j => j.JokeId == jokeId)
                .Select(j => new
                {
                    j.JokeId,
                    j.Text,
                    ClassificationName = j.Classification != null ? j.Classification.Name : null,
                    j.ClassificationId,
                    SourceName = j.Source != null ? j.Source.SourceName : null,
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
                    c.CommentId,
                    c.CommentDate,
                    Ratings = c.CommentRatings,
                    c.ParentCommentId
                })
                .ToListAsync();

            var groupedComments = comments.Select(c => new CommentDto
            {
                JokeId = c.JokeId,
                CommentId = c.CommentId,
                ParentCommentId = c.ParentCommentId,
                CommentDate = c.CommentDate,
                CommentText = c.CommentText,
                Username = c.Username ?? "Unknown",
                TotalLikes = c.Ratings.Count(r => r.Rating),
                TotalDislikes = c.Ratings.Count(r => !r.Rating)
            }).ToList();

            var jokeDto = new JokeDto
            {
                JokeId = baseJoke.JokeId,
                Text = baseJoke.Text,
                ClassificationName = baseJoke.ClassificationName ?? "Unknown",
                ClassificationId = baseJoke.ClassificationId,
                SourceName = baseJoke.SourceName ?? "Unknown",
                SourceId = baseJoke.SourceId,
                TotalLikes = ratings.FirstOrDefault(r => r.JokeId == baseJoke.JokeId)?.Likes ?? 0,
                TotalDislikes = ratings.FirstOrDefault(r => r.JokeId == baseJoke.JokeId)?.Dislikes ?? 0,
                Comments = groupedComments.BuildHierarchicalComments()
            };

            return jokeDto;
        }
        public async Task<Joke> CreateJokeAsync(JokeCreateDto? jokeCreateDto, string userId)
        {
            if (jokeCreateDto == null || string.IsNullOrWhiteSpace(jokeCreateDto.Text))
            {
                throw new ArgumentException("Joke content cannot be empty.");
            }

            var joke = jokeCreateDto.ToJokeFromCreateDto(userId);

            var res = await _context.Jokes.AddAsync(joke);
            await _context.SaveChangesAsync();

            return res.Entity;
        }
        public async Task<Joke> UpdateJokeAsync(int id, JokeUpdateDto? jokeUpdateDto)
        {
            var joke = await _context.Jokes.FindAsync(id);
            if (joke == null)
            {
                throw new KeyNotFoundException($"Joke with ID {id} not found.");
            }
            if (jokeUpdateDto == null)
            {
                throw new ArgumentException("Joke update cannot be empty.");
            }
            joke = joke.UpdateJokeFromJokeDto(jokeUpdateDto);
            _context.Jokes.Update(joke);
            await _context.SaveChangesAsync();
            return joke;
        }

        public async Task<Joke> UpdateJokeByUserAsync(int id, JokeUpdateDto? jokeUpdateDto)
        {
            var joke = await _context.Jokes.FindAsync(id);
            if (joke == null)
            {
                throw new KeyNotFoundException($"Joke with ID {id} not found.");
            }
            if (jokeUpdateDto == null)
            {
                throw new ArgumentException("Joke update cannot be empty.");
            }
            joke.IsApproved = false;
            joke = joke.UpdateJokeFromJokeDto(jokeUpdateDto);
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

        public async Task<bool> IsJokeOwnerAsync(int id, string userId)
        {
            return await _context.Jokes.AnyAsync(j => j.JokeId == id && j.SubbmitedByUserId == userId);
        }

        public async Task<JokeDto> GetRandomJokeAsync(List<int> viewedJokes)
        {

            var jokeIds = await _context.Jokes.Where(j => j.SourceId != -4).Select(j => j.JokeId).ToListAsync();

            if (jokeIds == null || jokeIds.Count == 0)
            {
                throw new InvalidOperationException("No jokes available to select from.");
            }

            var random = new Random();
            var randomIndex = random.Next(0, jokeIds.Count);
            if (viewedJokes is { Count: > 0 })
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

        public async Task<JokeDto> GetRandomJokeAsync(List<int> viewedJokes, RandomJokeQueryObject query)
        {
            var jokeIds = await _context.Jokes
                .Where(j =>
                    j.IsApproved == true &&
                    (
                        (!query.SourceIds.Any() && !query.ClassificationIds.Any()) || 
                        (query.SourceIds.Any() && query.SourceIds.Contains(j.SourceId)) || 
                        (query.ClassificationIds.Any() && j.ClassificationId.HasValue && query.ClassificationIds.Contains(j.ClassificationId.Value)) 
                    )
                )
                .Select(j => j.JokeId)
                .ToListAsync();


        
            if (jokeIds == null || jokeIds.Count == 0)
            {
                throw new InvalidOperationException("No jokes available to select from.");
            }
        
            var random = new Random();
            var randomIndex = random.Next(0, jokeIds.Count);
        
            if (viewedJokes is { Count: > 0 })
            {
                jokeIds = jokeIds.Where(id => !viewedJokes.Contains(id)).ToList();
                if (jokeIds.Count == 0)
                {
                    throw new InvalidOperationException("No unviewed jokes available to select from.");
                }
                randomIndex = random.Next(0, jokeIds.Count);
            }
        
            var randomJokeId = jokeIds[randomIndex];
        
            return await GetJokeByIdAsync(randomJokeId) ?? throw new KeyNotFoundException($"Joke not found.");
        }

        public async Task<List<JokeDto>> GetJokesByIdsAsync(List<int> ids)
        {
            var ids1 = ids;
            var baseJokes = await _context.Jokes
                .AsNoTracking()
                .Where(j => ids1.Contains(j.JokeId))
                .OrderByDescending(j => j.SubbmissionDate)
                .Select(j => new
                {
                    j.JokeId,
                    j.Text,
                    ClassificationName = j.Classification != null ? j.Classification.Name : null,
                    j.ClassificationId,
                    SourceName = j.Source != null ? j.Source.SourceName : null,
                    j.SourceId
                })
                .ToListAsync();
            ids = baseJokes.Select(j => j.JokeId).ToList();
            var ratings = await _context.JokeRatings
                .AsNoTracking()
                .Where(r => ids.Contains(r.JokeId))
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
                .Where(c => ids.Contains(c.JokeId))
                .Select(c => new
                {
                    c.JokeId,
                    c.CommentText,
                    c.CommentDate,
                    Username = c.User.UserName,
                    c.CommentId,
                    Ratings = c.CommentRatings,
                    c.ParentCommentId
                })
                .ToListAsync();

            var groupedCommentsDict = comments
                    .GroupBy(c => c.JokeId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(c => new CommentDto
                        {
                            JokeId = c.JokeId,
                            CommentId = c.CommentId,
                            ParentCommentId = c.ParentCommentId,
                            CommentDate = c.CommentDate,
                            CommentText = c.CommentText,
                            Username = c.Username ?? "Unknown",
                            TotalLikes = c.Ratings.Count(r => r.Rating),
                            TotalDislikes = c.Ratings.Count(r => !r.Rating)
                        }).ToList()
                    );

            var jokeDtOs = baseJokes.Select(j => new JokeDto
            {
                JokeId = j.JokeId,
                Text = j.Text,
                ClassificationName = j.ClassificationName ?? "Unknown",
                ClassificationId = j.ClassificationId,
                SourceName = j.SourceName ?? "Unknown",
                SourceId = j.SourceId,
                TotalLikes = ratings.FirstOrDefault(r => r.JokeId == j.JokeId)?.Likes ?? 0,
                TotalDislikes = ratings.FirstOrDefault(r => r.JokeId == j.JokeId)?.Dislikes ?? 0,
                Comments = groupedCommentsDict.TryGetValue(j.JokeId, out List<CommentDto>? jokeComments)
                    ? jokeComments.BuildHierarchicalComments()
                    : new List<CommentDto>()
            }).ToList();

            return jokeDtOs;
        }

        public async Task<List<JokePreviewDto>> GetSuggestedByMeJokes(string userId)
        {
            var result = await _context.Jokes
                .AsNoTracking()
                .Where(j => j.SourceId == -4 && j.SubbmitedByUserId == userId)
                .OrderByDescending(j => j.SubbmissionDate)
                .Select(j => new JokePreviewDto
                {
                    JokeId = j.JokeId,
                    Text = j.Text,
                    ClassificationName = j.Classification != null ? j.Classification.Name : "Unknown",
                    Source = j.Source != null ? j.Source.SourceName : "Unknown",
                    IsApproved = j.IsApproved,
                    LikeCount = j.JokeRatings.Count(r => r.Rating),
                    DislikeCount = j.JokeRatings.Count(r => !r.Rating),
                    CommentCount = j.Comments.Count(),
                    SubmissionDate = j.SubbmissionDate
                })
                .ToListAsync();

            return result;
        }
    }
}