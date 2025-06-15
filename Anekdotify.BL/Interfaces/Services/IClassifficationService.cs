using Anekdotify.Common;
using Anekdotify.Models.DTOs.Classification;
using Anekdotify.Models.Entities;

namespace Anekdotify.BL.Interfaces.Services;

public interface IClassifficationService
{
    Task<OperationResult<Classification>> CreateClassificationAsync(string classificationName);
    Task<OperationResult<List<ClassificationDetailedDTO>>> GetAllClassificationsAsync();
    Task<OperationResult<Classification>> GetClassificationByNameAsync(string classificationName);
    Task<OperationResult<ClassificationDTO>> GetClassificationByIdAsync(int classificationId);
    Task<OperationResult<ClassificationDTO>> UpdateClassificationAsync(int classificationId, string classificationName);
    Task<OperationResult> DeleteClassificationAsync(int classificationId);
    Task<bool> IsExistingAsync(string classificationName);
}
