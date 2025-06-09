

using frontend.Comments.DTOs;
using frontend.Models;

namespace frontend.Mappers;

public static class CommentMappers
{
    public static Comment ToCommentFromCommentDTO(this CommentDTO commentDTO, int jokeId)
    {
        return new Comment()
        {
            CommentText = commentDTO.CommentText,
            JokeId = jokeId,
            Username = commentDTO.Username,
            ParentCommentId = commentDTO.ParentCommentId,
            Replies = commentDTO.Replies,
            TotalLikes = commentDTO.TotalLikes,
            TotalDislikes = commentDTO.TotalLikes
        };
    }

    public static Comment ToCommentFromCreateDTO(this CommentCreateDTO createDTO, int id)
    {
        return new Comment()
        {
            CommentText = createDTO.Text,
            ParentCommentId = createDTO.ParentCommentId,
            JokeId = createDTO.JokeId,
        };
    }
    public static Comment UpdateCommentFromEditDTO(this Comment comment, CommentEditDTO commnetEditDTO)
    {
        if (!string.IsNullOrEmpty(commnetEditDTO.Text))
        {
            comment.CommentText = commnetEditDTO.Text;
        }
        return comment;
    }

    public static CommentCreateDTO ToCommentCreateDTOFromComment(this Comment comment)
    {
        return new CommentCreateDTO()
        {
            Text = comment.CommentText
        };
    }

    public static CommentEditDTO ToEditDTOFromCreateDTO(this CommentCreateDTO commentCreateDTO, int id)
    {
        return new CommentEditDTO()
        {
            Text = commentCreateDTO.Text
        };
    }

    public static CommentEditDTO ToCommentEditDTO(this Comment comment)
    {
        return new CommentEditDTO()
        {
            Text = comment.CommentText
        };
    }
}
