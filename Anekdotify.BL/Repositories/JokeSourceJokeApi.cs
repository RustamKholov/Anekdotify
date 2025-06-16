using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.Entities;
using Anekdotify.Models.Models;
using Newtonsoft.Json;

namespace Anekdotify.BL.Repositories
{
    public class JokeSourceJokeApi : IJokeSource
    {
        private readonly HttpClient _httpClient;
        private readonly IClassifficationService _classifficationService;
        private readonly ISourceFetchedJokesService _sourceFetchedService;
        private List<int> _fetchedJokes = new List<int>();
        public JokeSourceJokeApi(HttpClient httpClient,
        IClassifficationService classifficationService, ISourceFetchedJokesService sourceFetchedService)
        {
            _httpClient = httpClient;
            _classifficationService = classifficationService;
            _sourceFetchedService = sourceFetchedService;
        }
        public async Task<JokeCreateDTO?> GetJokeAsync()
        {
            if (_fetchedJokes.Count == 0)
            {
                var allreadyFetchedJokesRes = await _sourceFetchedService.GetAllSourceFetchedJokesAsync();
                _fetchedJokes = allreadyFetchedJokesRes.Value ?? new List<int>();
            }

            var response = await _httpClient.GetAsync("joke/Any");
            if(!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();

            var parsed = JsonConvert.DeserializeObject<JokeApiResponse>(content);

            var isTwoParted = parsed?.Type == "twopart";

            if (parsed == null || (parsed.Joke == null && parsed.Setup == null && parsed.Delivery == null))
            {
                return null;
            }
            if (_fetchedJokes.Any(fj => fj == parsed.Id))
            {
                return null; // Joke already fetched
            }

            Classification classification;
            string categoryName = parsed.Category ?? "General";

            var getClassificationResult = await _classifficationService.GetClassificationByNameAsync(categoryName);
            if (getClassificationResult.IsSuccess)
            {
                classification = getClassificationResult.Value!;
            }
            else
            {
                var createClassificationResult = await _classifficationService.CreateClassificationAsync(categoryName);
                if (createClassificationResult.IsSuccess)
                {
                    classification = createClassificationResult.Value!;
                }
                else
                {
                    return null; 
                }
            }

            await _sourceFetchedService.AddSourceFetchedJokeAsync(1, parsed.Id ?? 0);
            _fetchedJokes.Add(parsed.Id ?? 0);

            return new JokeCreateDTO
                {
                    Text = isTwoParted ? parsed.Setup + "\n" + parsed.Delivery : parsed.Joke,
                    ClassificationId = classification.ClassificationId,
                    SourceId = 1
                };
        }
    }
}
