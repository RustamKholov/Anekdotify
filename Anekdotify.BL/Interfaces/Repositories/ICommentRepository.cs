using Anekdotify.BL.Helpers;
using Anekdotify.Models.DTOs.Comments;
using Anekdotify.Models.Entities;

namespace Anekdotify.BL.Interfaces.Repositories
{
    public interface ICommentRepository
    {
        Task<List<CommentDTO>> GetAllCommentsAsync(CommentsQueryObject query);
        Task<Comment?> GetCommentByIdAsync(int id);
        Task<Comment> CreateCommentAsync(Comment comment);
        Task<Comment?> UpdateCommentAsync(int id, CommentUpdateDTO comment);
        Task<Comment?> DeleteCommentAsync(int id);
        Task<bool> CommentExistsAsync(int id);

    }

}