using System;
using frontend.DTOs;
using frontend.Mappers;
using frontend.Models;

namespace frontend.Clients;

public class CommentClient
{
    private readonly List<Comment> _comments = [];


    public CommentClient()
    {
        _comments = new(){
            new Comment()
            {
                ID = 1,
                Title = "First Title",
                Content = "The Content",
                JokeID = 1
            },
            new Comment()
            {
                ID = 1,
                Title = "Second Title",
                Content = "The Content",
                JokeID = 1
            },
            new Comment()
            {
                ID = 1,
                Title = "Third Title",
                Content = "The Content",
                JokeID = 2
            },
            new Comment()
            {
                ID = 1,
                Title = "Fourth Title",
                Content = "The Content",
                JokeID = 2
            },
            new Comment()
            {
                ID = 1,
                Title = "Fifth Title",
                Content = "The Content",
                JokeID = 3
            },
            new Comment()
            {
                ID = 1,
                Title = "Sixth Title",
                Content = "The Content",
                JokeID = 3
            },
        };
    }

    public Comment[] GetComments() => _comments.ToArray();

    public void AddComment(CommentCreateDTO commentCreateDTO, int jokeId)
    {
        _comments.Add(commentCreateDTO.ToCommentFromCreateDTO(jokeId, id: _comments.Count + 1));
    }
}
