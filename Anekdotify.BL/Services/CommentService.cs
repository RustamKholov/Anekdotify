using Anekdotify.BL.Helpers;
using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Models.DTOs.Comments;
using Anekdotify.Models.Entities;

namespace Anekdotify.BL.Services;

public class CommentService(ICommentRepository commentRepository) : ICommentService
{
    public async Task<Comment> CreateCommentAsync(Comment comment)
    {
        return await commentRepository.CreateCommentAsync(comment);
    }

    public async Task<Comment?> DeleteCommentAsync(int id)
    {
        return await commentRepository.DeleteCommentAsync(id);
    }

    public async Task<List<CommentDTO>> GetAllCommentsAsync(CommentsQueryObject query)
    {
        return await commentRepository.GetAllCommentsAsync(query);
    }

    public async Task<Comment?> GetCommentByIdAsync(int id)
    {
        return await commentRepository.GetCommentByIdAsync(id);
    }

    public async Task<Comment?> UpdateCommentAsync(int id, CommentUpdateDTO commentUpdateDTO)
    {
        return await commentRepository.UpdateCommentAsync(id, commentUpdateDTO);
    }
}
