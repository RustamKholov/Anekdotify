using Anekdotify.Models.DTOs.Comments;
using Anekdotify.Models.Entities;

namespace Anekdotify.BL.Mappers
{
    public static class CommentMappers
    {
        public static CommentDto ToCommentDto(this Comment commentModel)
        {
            int totalLikes = commentModel.CommentRatings.Count(cr => cr.Rating);
            int totalDislikes = commentModel.CommentRatings.Count(cr => !cr.Rating);
            return new CommentDto
            {
                CommentId = commentModel.CommentId,
                CommentText = commentModel.CommentText,
                CommentDate = commentModel.CommentDate,
                JokeId = commentModel.JokeId,
                Username = commentModel.User.UserName ?? "Unknown Username",
                ParentCommentId = commentModel.ParentCommentId,
                TotalLikes = totalLikes,
                TotalDislikes = totalDislikes
            };
        }
        public static Comment ToCommentFromCreateDto(this CommentCreateDto commentCreateDto, int jokeId, string userId)
        {
            return new Comment
            {
                CommentText = commentCreateDto.CommentText,
                JokeId = jokeId,
                UserId = userId,
                ParentCommentId = commentCreateDto.ParentCommentId
            };
        }

        public static List<CommentDto> BuildHierarchicalComments(this List<CommentDto> flatComments)
        {
            var allCommentsDtOs = flatComments.ToDictionary(dto => dto.CommentId);

            var rootComments = new List<CommentDto>();

            foreach (var dto in allCommentsDtOs.Values)
            {
                if (dto.ParentCommentId.HasValue && allCommentsDtOs.TryGetValue(dto.ParentCommentId.Value, out var parentDto))
                {
                    parentDto.Replies.Add(dto);
                }
                else
                {
                    rootComments.Add(dto);
                }
            }
            return rootComments.OrderBy(c => c.CommentDate).ToList();
        }
    }
}