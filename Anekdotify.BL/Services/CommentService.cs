using Anekdotify.BL.Helpers;
using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Models.DTOs.Comments;
using Anekdotify.Models.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Anekdotify.BL.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly ILogger<CommentService> _logger;

    public CommentService(ICommentRepository commentRepository, ILogger<CommentService> logger)
    {
        _commentRepository = commentRepository;
        _logger = logger;
    }

    public async Task<bool> CommentExistsAsync(int id)
    {
        var exists = await _commentRepository.CommentExistsAsync(id);
        _logger.LogInformation("Checked existence for comment {CommentId}: {Exists}", id, exists);
        return exists;
    }

    public async Task<bool> IsCommentOwnerAsync(int id, string userId)
    {
        var isOwner = await _commentRepository.IsCommentOwnerAsync(id, userId);
        _logger.LogInformation("Checked ownership for comment {CommentId} by user {UserId}: {IsOwner}", id, userId, isOwner);
        return isOwner;
    }

    public async Task<Comment> CreateCommentAsync(Comment comment)
    {
        try
        {
            var created = await _commentRepository.CreateCommentAsync(comment);
            _logger.LogInformation("Created comment {CommentId} by user {UserId}", created.CommentId, created.UserId);
            return created;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating comment by user {UserId}", comment.UserId);
            throw;
        }
    }

    public async Task<Comment?> DeleteCommentAsync(int id)
    {
        try
        {
            var comment = await _commentRepository.DeleteCommentAsync(id);
            if (comment == null)
            {
                _logger.LogWarning("Attempted to delete non-existent comment {CommentId}", id);
                return null;
            }
            _logger.LogInformation("Deleted comment {CommentId} by user {UserId}", comment.CommentId, comment.UserId);
            return comment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment {CommentId}", id);
            throw;
        }
    }

    public async Task<List<CommentDto>> GetAllCommentsAsync(CommentsQueryObject query)
    {
        try
        {
            var comments = await _commentRepository.GetAllCommentsAsync(query);
            _logger.LogInformation("Fetched {Count} comments for query {@Query}", comments.Count, query);
            return comments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching comments for query {@Query}", query);
            throw;
        }
    }

    public async Task<CommentDto?> GetCommentByIdAsync(int id)
    {
        try
        {
            var comment = await _commentRepository.GetCommentByIdAsync(id);
            if (comment == null)
            {
                _logger.LogWarning("Comment not found for id {CommentId}", id);
                return null;
            }
            _logger.LogInformation("Fetched comment {CommentId}", id);
            return comment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching comment by id {CommentId}", id);
            throw;
        }
    }

    public async Task<Comment?> UpdateCommentAsync(int id, CommentUpdateDto commentUpdateDto)
    {
        try
        {
            var comment = await _commentRepository.UpdateCommentAsync(id, commentUpdateDto);
            if (comment == null)
            {
                _logger.LogWarning("Attempted to update non-existent comment {CommentId}", id);
                return null;
            }
            _logger.LogInformation("Updated comment {CommentId} by user {UserId}", comment.CommentId, comment.UserId);
            return comment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating comment {CommentId}", id);
            throw;
        }
    }
}