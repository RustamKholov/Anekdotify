using System;
using frontend.DTOs;
using frontend.Mappers;
using frontend.Models;

namespace frontend.Clients;

public class JokesClient
{
    private readonly List<Joke> jokes =
    [
        new(){
            Id = 1,
            Title = "title",
            Content = "Some content",
            Comments = []
        },
        new(){
            Id = 2,
            Title = "newTitile",
            Content = "Some other content",
            Comments = []
        },
        new(){
            Id = 3,
            Title = "oldTitle",
            Content = "Some previous content",
            Comments = []
        },
    ];

    public Joke[] GetJokes() => jokes.ToArray();

    public void AddGame(JokeCreateDTO jokeCreateDTO)
    {
        jokes.Add(jokeCreateDTO.ToJokeFromCreateDTO(id:jokes.Count + 1));
    }
}
