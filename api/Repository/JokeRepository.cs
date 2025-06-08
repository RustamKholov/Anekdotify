using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using api.Data;
using api.DTOs.Jokes;
using api.Helpers;
using api.Interfaces;
using api.Mappers;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace api.Repository
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
                .AsQueryable();
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
            .Where(j => j.JokeId == jokeId) // Only approved jokes
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
    }
}