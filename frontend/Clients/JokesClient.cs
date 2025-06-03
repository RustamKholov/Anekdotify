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

    public void AddJoke(JokeCreateDTO jokeCreateDTO)
    {
        jokes.Add(jokeCreateDTO.ToJokeFromCreateDTO(id: jokes.Count + 1));
    }

    public Joke? GetJoke(int id)
    {
        return FindJoke(id);
    }

    private Joke? FindJoke(int id)
    {
        return jokes.Find(joke => joke.Id == id);
    }
    public void UpdateJoke(JokeEditDTO jokeEditDTO)
    {
        var existingJoke = FindJoke(jokeEditDTO.Id) ?? throw new Exception("Joke does not exist");
        existingJoke.UpdateJokeFromEditDTO(jokeEditDTO);
    }

    public void PopulateComments(Comment[] comments)
    {
        foreach (var joke in jokes)
        {
            joke.Comments.Clear();
            foreach (var comment in comments)
            {
                if (comment.JokeID == joke.Id)
                {
                    if (!joke.Comments.Contains(comment))
                    {
                        joke.Comments.Add(comment);
                    }
                }
            }
        }
    }

    public List<int> GetIDs()
    {
        var list = new List<int>();
        foreach (var joke in jokes)
        {
            list.Add(joke.Id);
        }
        return list;
    }
    public string? GetTitle(int id)
    {
        return FindJoke(id)?.Title;
    }
    public void DeleteJoke(int id)
    {
        var joke = FindJoke(id);
        if (joke != null)
        {
            jokes.Remove(joke);
        }
    }
}
