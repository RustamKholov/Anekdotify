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

    public static Comment ToCommentFromCreateDTO(this CommentCreateDTO createDTO, int jokeId, int id)
    {
        return new Comment()
        {
            ID = id,
            Title = createDTO.Title,
            Content = createDTO.Content,
            JokeID = jokeId
        };
    }
}
