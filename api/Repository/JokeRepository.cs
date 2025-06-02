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
        public async Task<List<Joke>> GetAllJokesAsync(JokesQueryObject query)
        {
            var jokes = _context.Jokes
                .Include(j => j.Comments)
                .AsQueryable();
            if (query.AddingDay.HasValue)
            {
                var day = query.AddingDay.Value.Date;
                var nextDay = day.AddDays(1);
                jokes = jokes.Where(j => j.AddedAt >= day && j.AddedAt < nextDay);
            }
            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                if (string.Equals(query.SortBy, "AddingDay", StringComparison.OrdinalIgnoreCase))
                {
                    jokes = query.ByDescending ? jokes.OrderByDescending(j => j.AddedAt) : jokes.OrderBy(j => j.AddedAt);
                }
            }
            return await jokes.ToListAsync();
        }
        public async Task<Joke?> GetJokeByIdAsync(int id)
        {
            return await _context.Jokes
                .Include(j => j.Comments)
                .FirstOrDefaultAsync(j => j.Id == id);
        }
        public async Task<Joke> CreateJokeAsync(JokeCreateDTO jokeCreateDTO)
        {
            if (jokeCreateDTO == null || string.IsNullOrWhiteSpace(jokeCreateDTO.Content))
            {
                throw new ArgumentException("Joke content cannot be empty.");
            }

            var joke = jokeCreateDTO.ToJokeFromCreateDTO();

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
            await _context.SaveChangesAsync();
            return joke;
        }
        public async Task<List<Comment>> GetCommentsByJokeIdAsync(int jokeId)
        {
            return await _context.Comments
                .Where(c => c.JokeId == jokeId)
                .ToListAsync();
        }

        public Task<bool> JokeExists(int id)
        {
            return _context.Jokes.AnyAsync(j => j.Id == id);
        }
    }
}