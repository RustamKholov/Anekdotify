using Anekdotify.BL.Helpers;
using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Models.DTOs.Comments;
using Anekdotify.Models.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Anekdotify.BL.Services;

public class CommentService(ICommentRepository commentRepository) : ICommentService
{
    public async Task<bool> CommentExistsAsync(int id)
    {
        return await commentRepository.CommentExistsAsync(id);
    }

    public async Task<bool> IsCommentOwnerAsync(int id, string userId)
    {
        return await commentRepository.IsCommentOwnerAsync(id, userId);
    }

    public async Task<Comment> CreateCommentAsync(Comment comment)
    {
        return await commentRepository.CreateCommentAsync(comment);
    }

    public async Task<Comment?> DeleteCommentAsync(int id)
    {
        var comment = await commentRepository.DeleteCommentAsync(id);
        if (comment == null)
        {
            return null;
        }
        return comment;
    }

    public async Task<List<CommentDto>> GetAllCommentsAsync(CommentsQueryObject query)
    {

        var comments = await commentRepository.GetAllCommentsAsync(query);

        return comments;
    }

    public async Task<CommentDto?> GetCommentByIdAsync(int id)
    {
        var comment = await commentRepository.GetCommentByIdAsync(id);

        if (comment == null)
        {
            return null;
        }

        return comment;
    }

    public async Task<Comment?> UpdateCommentAsync(int id, CommentUpdateDto commentUpdateDto)
    {
        var comment = await commentRepository.UpdateCommentAsync(id, commentUpdateDto);

        if (comment == null)
        {
            return null;
        }

        return comment;
    }
}
