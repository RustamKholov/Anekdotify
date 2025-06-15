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
        public JokeSourceJokeApi(HttpClient httpClient,
        IClassifficationService classifficationService, ISourceFetchedJokesService sourceFetchedService)
        {
            _httpClient = httpClient;
            _classifficationService = classifficationService;
            _sourceFetchedService = sourceFetchedService;
        }
        public async Task<JokeCreateDTO?> GetJokeAsync()
        {
            var response = await _httpClient.GetAsync("joke/Any?type=single");
            if(!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var parsed = JsonConvert.DeserializeObject<JokeApiResponse>(content);
            
            if (parsed == null || parsed.Joke == null)
            {
                return null;
            }
            var allreadyFetchedRes = await _sourceFetchedService.IsJokeFetchedFromSourceAsync(1, parsed.Id ?? 0);
            if (allreadyFetchedRes.Value)
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

            return new JokeCreateDTO
                {
                    Text = parsed.Joke,
                    ClassificationId = classification.ClassificationId,
                    SourceId = 1
                };
        }
    }
}
