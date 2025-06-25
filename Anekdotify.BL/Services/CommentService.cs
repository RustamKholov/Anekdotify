using Anekdotify.BL.Helpers;
using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Models.DTOs.Comments;
using Anekdotify.Models.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Anekdotify.BL.Services;

public class CommentService(ICommentRepository commentRepository, IDistributedCache cacheService) : ICommentService
{
    public async Task<bool> CommentExistsAsync(int id)
    {
        return await commentRepository.CommentExistsAsync(id);
    }

    public async Task<Comment> CreateCommentAsync(Comment comment)
    {
        await cacheService.RemoveAsync($"comments_joke_{comment.JokeId}"); 
        return await commentRepository.CreateCommentAsync(comment);
    }

    public async Task<Comment?> DeleteCommentAsync(int id)
    {
        var comment = await commentRepository.GetCommentByIdAsync(id);
        if (comment == null)
        {
            return null;
        }

        await cacheService.RemoveAsync($"comments_joke_{comment.JokeId}");
        await cacheService.RemoveAsync($"comment_{id}");

        return comment;
    }

    public async Task<List<CommentDTO>> GetAllCommentsAsync(CommentsQueryObject query)
    {
        var cacheValue = await cacheService.GetStringAsync($"comments_joke_{query.JokeId}");

        if (cacheValue != null)
        {
            return JsonConvert.DeserializeObject<List<CommentDTO>>(cacheValue) ?? new List<CommentDTO>();
        }
        var comments = await commentRepository.GetAllCommentsAsync(query);

        await cacheService.SetStringAsync($"comments_joke_{query.JokeId}", JsonConvert.SerializeObject(comments));

        return comments;
    }

    public async Task<Comment?> GetCommentByIdAsync(int id)
    {
        var cacheValue = await cacheService.GetStringAsync($"comment_{id}");
        if (cacheValue != null)
        {
            return JsonConvert.DeserializeObject<Comment>(cacheValue);
        }

        var comment = await commentRepository.GetCommentByIdAsync(id);

        if (comment == null)
        {
            return null;
        }

        await cacheService.SetStringAsync($"comment_{id}", JsonConvert.SerializeObject(comment));

        return comment;
    }

    public async Task<Comment?> UpdateCommentAsync(int id, CommentUpdateDTO commentUpdateDTO)
    {
        var comment = await commentRepository.UpdateCommentAsync(id, commentUpdateDTO);

        if (comment == null)
        {
            return null;
        }

        await cacheService.RemoveAsync($"comments_joke_{comment.JokeId}");
        await cacheService.RemoveAsync($"comment_{id}");

        return comment;
    }
}
