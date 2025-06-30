using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Common;

namespace Anekdotify.BL.Services
{
    public class SourceFetchedJokesService(ISourceFetchedJokesRepository sourceFetchedJokesRepository) : ISourceFetchedJokesService
    {
        public async Task<OperationResult> AddSourceFetchedJokeAsync(int sourceId, int sourceJokeId)
        {
            return await sourceFetchedJokesRepository.AddSourceFetchedJokeAsync(sourceId, sourceJokeId);
        }

        public async Task<OperationResult<List<int>>> GetAllSourceFetchedJokesAsync()
        {
            return await sourceFetchedJokesRepository.GetAllSourceFetchedJokesAsync();
        }

        public async Task<OperationResult<bool>> IsJokeFetchedFromSourceAsync(int sourceId, int sourceJokeId)
        {
            return await sourceFetchedJokesRepository.IsJokeFetchedFromSourceAsync(sourceId, sourceJokeId);
        }
    }
}
