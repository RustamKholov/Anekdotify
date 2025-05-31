using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTOs.Comments;
using api.Models;

namespace api.Mappers
{
    public static class CommentMappers
    {
        public static CommentDTO ToCommentDTO(this Comment commentModel)
        {
            return new CommentDTO
            {
                Id = commentModel.Id,
                Title = commentModel.Title,
                Content = commentModel.Content,
                CreatedAt = commentModel.CreatedAt,
                JokeID = commentModel.JokeId
            };
        }
        public static Comment ToCommentFromCreateDTO(this CommentCreateDTO commentDTO, int jokeId)
        {
            return new Comment
            {
                Title = commentDTO.Title,
                Content = commentDTO.Content,
                JokeId = jokeId
            };
        }

        public static Comment ToCommentFromUpdateDTO(this CommentUpdateDTO commentUpdateDTO)
        {
            return new Comment
            {
                Title = commentUpdateDTO.Title,
                Content = commentUpdateDTO.Content,
            };
        }
    }
}