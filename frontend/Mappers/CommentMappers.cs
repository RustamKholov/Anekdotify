using System;
using frontend.DTOs;
using frontend.Models;

namespace frontend.Mappers;

public static class CommentMappers
{
    public static Comment ToCommentFromCommentDTO(this CommentDTO commentDTO, int jokeId)
    {
        return new Comment()
        {
            Title = commentDTO.Title,
            Content = commentDTO.Content,
            JokeID = jokeId
        };
    }

    public static Comment ToCommentFromCreateDTO(this CommentCreateDTO createDTO, int id)
    {
        return new Comment()
        {
            ID = id,
            Title = createDTO.Title,
            Content = createDTO.Content,
            JokeID = createDTO.JokeID
        };
    }
    public static Comment UpdateCommentFromEditDTO(this Comment comment, CommentEditDTO commnetEditDTO)
    {
        if (!string.IsNullOrEmpty(commnetEditDTO.Title))
        {
            comment.Title = commnetEditDTO.Title;
        }
        if (!string.IsNullOrEmpty(commnetEditDTO.Content))
        {
            comment.Content = commnetEditDTO.Content;
        }
        return comment;
    }

    public static CommentCreateDTO ToCommentCreateDTOFromComment(this Comment comment)
    {
        return new CommentCreateDTO()
        {
            Title = comment.Title,
            Content = comment.Content,
            JokeID = comment.JokeID
        };
    }

    public static CommentEditDTO ToEditDTOFromCreateDTO(this CommentCreateDTO commentCreateDTO, int id)
    {
        return new CommentEditDTO()
        {
            Id = id,
            Title = commentCreateDTO.Title,
            Content = commentCreateDTO.Content
        };
    }

    public static CommentEditDTO ToCommentEditDTO(this Comment comment)
    {
        return new CommentEditDTO()
        {
            Id = comment.ID,
            Title = comment.Title,
            Content = comment.Content
        };
    }
}
