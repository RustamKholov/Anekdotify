using Anekdotify.Common;
using Anekdotify.Models.DTOs.Classification;

namespace Anekdotify.BL.Interfaces
{
    public interface IClassificationRepository
    {
        Task<OperationResult<ClassificationDTO>> CreateClassificationAsync(string classificationName);
        Task<OperationResult<List<ClassificationDetailedDTO>>> GetAllClassificationsAsync();
        Task<OperationResult<ClassificationDTO>> GetClassificationByIdAsync(int classificationId);
        Task<OperationResult<ClassificationDTO>> UpdateClassificationAsync(int classificationId, string classificationName);
        Task<OperationResult> DeleteClassificationAsync(int classificationId);
        Task<bool> IsExistingAsync(string classificationName);
    }
}