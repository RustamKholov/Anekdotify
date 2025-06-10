using System;
using Anekdotify.BL.Interfaces;
using Anekdotify.Common;
using Anekdotify.Models.DTOs.Classification;

namespace Anekdotify.BL.Services;

public class ClassifficationService(IClassificationRepository classificationRepository) : IClassifficationService
{
    public async Task<OperationResult<ClassificationDTO>> CreateClassificationAsync(string classificationName)
    {
        return await classificationRepository.CreateClassificationAsync(classificationName);
    }

    public async Task<OperationResult> DeleteClassificationAsync(int classificationId)
    {
        return await classificationRepository.DeleteClassificationAsync(classificationId);
    }

    public async Task<OperationResult<List<ClassificationDetailedDTO>>> GetAllClassificationsAsync()
    {
        return await classificationRepository.GetAllClassificationsAsync();
    }

    public async Task<OperationResult<ClassificationDTO>> GetClassificationByIdAsync(int classificationId)
    {
        return await classificationRepository.GetClassificationByIdAsync(classificationId);
    }

    public async Task<bool> IsExistingAsync(string classificationName)
    {
        return await classificationRepository.IsExistingAsync(classificationName);
    }

    public async Task<OperationResult<ClassificationDTO>> UpdateClassificationAsync(int classificationId, string classificationName)
    {
        return await classificationRepository.UpdateClassificationAsync(classificationId, classificationName);
    }
}
