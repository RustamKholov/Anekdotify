using Anekdotify.Models.DTOs.Comments;
using Anekdotify.Models.Entities;

namespace Anekdotify.BL.Mappers
{
    public static class CommentMappers
    {
        public static CommentDTO ToCommentDTO(this Comment commentModel)
        {
            int totalLikes = commentModel.CommentRatings?.Count(cr => cr.Rating) ?? 0;
            int totalDislikes = commentModel.CommentRatings?.Count(cr => !cr.Rating) ?? 0;
            return new CommentDTO
            {
                CommentId = commentModel.CommentId,
                CommentText = commentModel.CommentText,
                CommentDate = commentModel.CommentDate,
                JokeId = commentModel.JokeId,
                Username = commentModel.User?.UserName ?? "Unknown Username",
                ParentCommentId = commentModel.ParentCommentId,
                TotalLikes = totalLikes,
                TotalDislikes = totalDislikes
            };
        }
        public static Comment ToCommentFromCreateDTO(this CommentCreateDTO? commentCreateDTO, int jokeId, string userId)
        {
            return new Comment
            {
                CommentText = commentCreateDTO.CommentText,
                JokeId = jokeId,
                UserId = userId,
                ParentCommentId = commentCreateDTO.ParentCommentId
            };
        }

        public static List<CommentDTO> BuildHierarchicalComments(this List<CommentDTO> flatComments)
        {
            var allCommentsDTOs = flatComments.ToDictionary(dto => dto.CommentId);

            var rootComments = new List<CommentDTO>();

            foreach (var dto in allCommentsDTOs.Values)
            {
                if (dto.ParentCommentId.HasValue && allCommentsDTOs.TryGetValue(dto.ParentCommentId.Value, out var parentDto))
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