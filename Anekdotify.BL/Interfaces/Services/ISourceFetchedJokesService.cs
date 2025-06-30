using Anekdotify.Common;

namespace Anekdotify.BL.Interfaces.Services
{
    public interface ISourceFetchedJokesService
    {
        Task<OperationResult> AddSourceFetchedJokeAsync(int sourceId, int sourceJokeId);
        Task<OperationResult<bool>> IsJokeFetchedFromSourceAsync(int sourceId, int sourceJokeId);
        Task<OperationResult<List<int>>> GetAllSourceFetchedJokesAsync();
    }
}
