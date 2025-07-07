using Anekdotify.BL.Helpers;
using Anekdotify.Models.DTOs.Comments;
using Anekdotify.Models.Entities;

namespace Anekdotify.BL.Interfaces.Repositories
{
    public interface ICommentRepository
    {
        Task<List<CommentDto>> GetAllCommentsAsync(CommentsQueryObject query);
        Task<CommentDto?> GetCommentByIdAsync(int id);
        Task<Comment> CreateCommentAsync(Comment comment);
        Task<Comment?> UpdateCommentAsync(int id, CommentUpdateDto comment);
        Task<Comment?> DeleteCommentAsync(int id);
        Task<bool> CommentExistsAsync(int id);
        Task<bool> IsCommentOwnerAsync(int id, string userId);

    }

}