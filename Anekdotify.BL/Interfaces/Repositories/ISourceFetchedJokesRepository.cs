using Anekdotify.Common;

namespace Anekdotify.BL.Interfaces.Repositories
{
    public interface ISourceFetchedJokesRepository
    {
        Task<OperationResult> AddSourceFetchedJokeAsync(int sourceId, int sourceJokeId);
        Task<OperationResult<bool>> IsJokeFetchedFromSourceAsync(int sourceId, int sourceJokeId);
        Task<OperationResult<List<int>>> GetAllSourceFetchedJokesAsync();
    }
}
