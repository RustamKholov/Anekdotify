using Anekdotify.BL.Helpers;
using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Mappers;
using Anekdotify.Database.Data;
using Anekdotify.Models.DTOs.Comments;
using Anekdotify.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Anekdotify.BL.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ApplicationDBContext _context;
        public CommentRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<bool> CommentExistsAsync(int id)
        {
            return await _context.Comments.AnyAsync(c => c.CommentId == id);
        }

        public async Task<bool> IsCommentOwnerAsync(int id, string userId)
        {
            return await _context.Comments.AnyAsync(c => c.CommentId == id && c.UserId == userId);
        }

        public async Task<Comment> CreateCommentAsync(Comment comment)
        {
            var res = await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
            return res.Entity;
        }

        public async Task<Comment?> DeleteCommentAsync(int id)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == id);
            if (comment == null)
            {
                return null;
            }
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<List<CommentDto>> GetAllCommentsAsync(CommentsQueryObject query)
        {
            var baseQuery = _context.Comments
                .AsNoTracking()
                .Where(c => (query.JokeId == null || c.JokeId == query.JokeId) &&
                            (query.UserId == null || c.UserId == query.UserId));

            baseQuery = query.ByDescending
                ? baseQuery.OrderByDescending(c => c.CommentDate)
                : baseQuery.OrderBy(c => c.CommentDate);

            var pagedComments = await baseQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(c => new CommentDto
                {
                    JokeId = c.JokeId,
                    CommentId = c.CommentId,
                    CommentText = c.CommentText,
                    CommentDate = c.CommentDate,
                    Username = c.User.UserName ?? "Unknown",
                    TotalLikes = c.CommentRatings.Count(r => r.Rating),
                    TotalDislikes = c.CommentRatings.Count(r => !r.Rating),
                    ParentCommentId = c.ParentCommentId
                })
                .ToListAsync();

            var rootComments = pagedComments.BuildHierarchicalComments();

            return rootComments;
        }

        public async Task<CommentDto?> GetCommentByIdAsync(int id)
        {
            var comment = await _context.Comments
                .AsNoTracking()
                .Include(c => c.User)
                .Include(c => c.CommentRatings)
                .FirstOrDefaultAsync(c => c.CommentId == id);

            if (comment == null)
                return null;

            return new CommentDto
            {
                JokeId = comment.JokeId,
                CommentId = comment.CommentId,
                CommentText = comment.CommentText,
                CommentDate = comment.CommentDate,
                Username = comment.User?.UserName ?? "Unknown",
                TotalLikes = comment.CommentRatings.Count(r => r.Rating),
                TotalDislikes = comment.CommentRatings.Count(r => !r.Rating),
                ParentCommentId = comment.ParentCommentId
            };
        }

        public async Task<Comment?> UpdateCommentAsync(int id, CommentUpdateDto commentUpdateDto)
        {
            var existingComment = await _context.Comments.FindAsync(id);
            if (existingComment == null)
            {
                return null;
            }
            existingComment.CommentText = commentUpdateDto.CommentText;
            await _context.SaveChangesAsync();
            return existingComment;
        }
    }
}