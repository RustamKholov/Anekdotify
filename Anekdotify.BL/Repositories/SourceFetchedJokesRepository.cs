using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.Common;
using Anekdotify.Database.Data;
using Anekdotify.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Anekdotify.BL.Repositories
{
    public class SourceFetchedJokesRepository(ApplicationDBContext context) : ISourceFetchedJokesRepository
    {
        public async Task<OperationResult> AddSourceFetchedJokeAsync(int sourceId, int sourceJokeId)
        {
            var sourceFetchedJoke = new SourceFetchedJoke
            {
                SourceId = sourceId,
                SourceJokeId = sourceJokeId
            };
            await context.SourceFetchedJokes.AddAsync(sourceFetchedJoke);
            await context.SaveChangesAsync();
            return OperationResult.Success();
        }

        public async Task<OperationResult<List<int>>> GetAllSourceFetchedJokesAsync()
        {
            var fetchedJokes = await context.SourceFetchedJokes.Select(sfj => sfj.SourceJokeId)
                .ToListAsync();
            return OperationResult<List<int>>.Success(fetchedJokes);
        }

        public async Task<OperationResult<bool>> IsJokeFetchedFromSourceAsync(int sourceId, int sourceJokeId)
        {
           var res = await context.SourceFetchedJokes
                .AnyAsync(sfj => sfj.SourceId == sourceId && sfj.SourceJokeId == sourceJokeId);
           return res
                ? OperationResult<bool>.Success(true)
                : OperationResult<bool>.Success(false);
        }
    }
}
