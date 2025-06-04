using System;
using System.Threading.Tasks;
using frontend.DTOs;
using frontend.Mappers;
using frontend.Models;

namespace frontend.Clients;

public class JokesClient(HttpClient httpClient)
{
    public async Task<Joke[]> GetJokesAsync()
    => await httpClient.GetFromJsonAsync<Joke[]>("api/joke") ?? [];

    public async Task AddJokeAsync(JokeCreateDTO jokeCreateDTO)
        => await httpClient.PostAsJsonAsync("api/joke", jokeCreateDTO);

    public async Task<Joke?> GetJokeAsync(int id)
        => await httpClient.GetFromJsonAsync<Joke>($"api/joke/{id}") ?? throw new Exception("joke not found");


    public async Task UpdateJokeAsync(JokeEditDTO jokeEditDTO)
        => await httpClient.PutAsJsonAsync($"api/joke/{jokeEditDTO.Id}", jokeEditDTO);

    public async Task DeleteJoke(int id)
        => await httpClient.DeleteAsync($"api/joke/{id}");
}
