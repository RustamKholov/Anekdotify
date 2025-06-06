using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.DTOs.Comments;
using api.Helpers;
using api.Interfaces;
using api.Mappers;
using api.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace api.Repository
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ApplicationDBContext _context;
        public CommentRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task<Comment> CreateCommentAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
            return comment;
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

        public async Task<List<CommentDTO>> GetAllCommentsAsync(CommentsQueryObject query)
        {
            var AllComments = _context.Comments.AsQueryable();
            if (query.JokeId != null)
            {
                AllComments = AllComments.Where(c => c.JokeId == query.JokeId)
                                        .Include(c => c.User)
                                        .OrderBy(c => c.CommentDate);
            }
            var comments = await AllComments.ToListAsync();
            var rootComments = comments.BuildHierarchicalComments();
            return rootComments;
        }

        public async Task<Comment?> GetCommentByIdAsync(int id)
        {
            return await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == id);
        }

        public async Task<Comment?> UpdateCommentAsync(int id, CommentUpdateDTO commentUpdateDTO)
        {
            var existingComment = await _context.Comments.FindAsync(id);
            if (existingComment == null)
            {
                return null;
            }
            //from update
            existingComment.CommentText = commentUpdateDTO.CommentText;
            await _context.SaveChangesAsync();
            return existingComment;
        }
    }
}