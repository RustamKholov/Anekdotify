using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTOs.Comments;
using api.Helpers;
using api.Models;

namespace api.Interfaces
{
    public interface ICommentRepository
    {
        Task<List<CommentDTO>> GetAllCommentsAsync(CommentsQueryObject query);
        Task<Comment?> GetCommentByIdAsync(int id);
        Task<Comment> CreateCommentAsync(Comment comment);
        Task<Comment?> UpdateCommentAsync(int id, CommentUpdateDTO comment);
        Task<Comment?> DeleteCommentAsync(int id);

    }

}