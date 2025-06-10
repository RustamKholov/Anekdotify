using Anekdotify.BL.Helpers;
using Anekdotify.Models.DTOs.Comments;
using Anekdotify.Models.Entities;

namespace Anekdotify.BL.Interfaces;

public interface ICommentService
{
    Task<List<CommentDTO>> GetAllCommentsAsync(CommentsQueryObject query);
    Task<Comment?> GetCommentByIdAsync(int id);
    Task<Comment> CreateCommentAsync(Comment comment);
    Task<Comment?> UpdateCommentAsync(int id, CommentUpdateDTO comment);
    Task<Comment?> DeleteCommentAsync(int id);
}
