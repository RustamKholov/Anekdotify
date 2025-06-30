using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Common;
using Anekdotify.Models.DTOs.Classification;
using Anekdotify.Models.Entities;

namespace Anekdotify.BL.Services;

public class ClassifficationService(IClassificationRepository classificationRepository) : IClassifficationService
{
    public async Task<OperationResult<Classification>> CreateClassificationAsync(string classificationName)
    {
        return await classificationRepository.CreateClassificationAsync(classificationName);
    }

    public async Task<OperationResult> DeleteClassificationAsync(int classificationId)
    {
        return await classificationRepository.DeleteClassificationAsync(classificationId);
    }

    public async Task<OperationResult<List<ClassificationDetailedDto>>> GetAllClassificationsAsync()
    {
        return await classificationRepository.GetAllClassificationsAsync();
    }

    public async Task<OperationResult<ClassificationDto>> GetClassificationByIdAsync(int classificationId)
    {
        return await classificationRepository.GetClassificationByIdAsync(classificationId);
    }

    public async Task<OperationResult<Classification>> GetClassificationByNameAsync(string classificationName)
    {
        return await classificationRepository.GetClassificationByNameAsync(classificationName);
    }

    public async Task<bool> IsExistingAsync(string classificationName)
    {
        return await classificationRepository.IsExistingAsync(classificationName);
    }

    public async Task<OperationResult<ClassificationDto>> UpdateClassificationAsync(int classificationId, string classificationName)
    {
        return await classificationRepository.UpdateClassificationAsync(classificationId, classificationName);
    }
}
