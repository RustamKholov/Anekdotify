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
                ID = 2,
                Title = "Second Title",
                Content = "The Content",
                JokeID = 1
            },
            new Comment()
            {
                ID = 3,
                Title = "Third Title",
                Content = "The Content",
                JokeID = 2
            },
            new Comment()
            {
                ID = 4,
                Title = "Fourth Title",
                Content = "The Content",
                JokeID = 2
            },
            new Comment()
            {
                ID = 5,
                Title = "Fifth Title",
                Content = "The Content",
                JokeID = 3
            },
            new Comment()
            {
                ID = 6,
                Title = "Sixth Title",
                Content = "The Content",
                JokeID = 3
            },
        };
    }

    public Comment[] GetComments() => _comments.ToArray();

    public void AddComment(CommentCreateDTO commentCreateDTO)
    {
        _comments.Add(commentCreateDTO.ToCommentFromCreateDTO(id: _comments.Count + 1));
    }
    public void UpdateComment(CommentEditDTO commentEditDTO)
    {
        if (commentEditDTO.Id is null) throw new Exception("no comment id");
        var existingComment = FindComment(commentEditDTO.Id.Value) ?? throw new Exception("comment not found");
        existingComment.UpdateCommentFromEditDTO(commentEditDTO);
    }
    public Comment? GetComment(int id)
    {
        return FindComment(id);
    }
    private Comment? FindComment(int id)
    {
        return _comments.Find(c => c.ID == id);
    }
}
