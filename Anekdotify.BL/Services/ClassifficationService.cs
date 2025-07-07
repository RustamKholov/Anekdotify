using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Common;
using Anekdotify.Models.DTOs.Classification;
using Anekdotify.Models.DTOs.Source;
using Anekdotify.Models.Entities;
using Microsoft.Extensions.Logging;

namespace Anekdotify.BL.Services;

public class ClassifficationService : IClassifficationService
{
    private readonly IClassificationRepository _classificationRepository;
    private readonly ILogger<ClassifficationService> _logger;

    public ClassifficationService(IClassificationRepository classificationRepository, ILogger<ClassifficationService> logger)
    {
        _classificationRepository = classificationRepository;
        _logger = logger;
    }

    public async Task<OperationResult<Classification>> CreateClassificationAsync(string classificationName)
    {
        try
        {
            var result = await _classificationRepository.CreateClassificationAsync(classificationName);
            _logger.LogInformation("Created classification: {Name} (Success: {Success})", classificationName, result.IsSuccess);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating classification: {Name}", classificationName);
            throw;
        }
    }

    public async Task<OperationResult> DeleteClassificationAsync(int classificationId)
    {
        try
        {
            var result = await _classificationRepository.DeleteClassificationAsync(classificationId);
            _logger.LogInformation("Deleted classification: {Id} (Success: {Success})", classificationId, result.IsSuccess);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting classification: {Id}", classificationId);
            throw;
        }
    }

    public async Task<OperationResult<List<ClassificationDetailedDto>>> GetAllClassificationsAsync()
    {
        try
        {
            var result = await _classificationRepository.GetAllClassificationsAsync();
            _logger.LogInformation("Fetched all classifications (Count: {Count})", result.Value?.Count ?? 0);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all classifications");
            throw;
        }
    }

    public async Task<OperationResult<List<SourceDto>>> GetAllSourcesAsync()
    {
        try
        {
            var result = await _classificationRepository.GetAllSourcesAsync();
            _logger.LogInformation("Fetched all sources (Count: {Count})", result.Value?.Count ?? 0);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all sources");
            throw;
        }
    }

    public async Task<OperationResult<ClassificationDto>> GetClassificationByIdAsync(int classificationId)
    {
        try
        {
            var result = await _classificationRepository.GetClassificationByIdAsync(classificationId);
            _logger.LogInformation("Fetched classification by id: {Id} (Found: {Found})", classificationId, result.Value != null);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching classification by id: {Id}", classificationId);
            throw;
        }
    }

    public async Task<OperationResult<Classification>> GetClassificationByNameAsync(string classificationName)
    {
        try
        {
            var result = await _classificationRepository.GetClassificationByNameAsync(classificationName);
            _logger.LogInformation("Fetched classification by name: {Name} (Found: {Found})", classificationName, result.Value != null);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching classification by name: {Name}", classificationName);
            throw;
        }
    }

    public async Task<bool> IsExistingAsync(string classificationName)
    {
        try
        {
            var exists = await _classificationRepository.IsExistingAsync(classificationName);
            _logger.LogInformation("Checked existence for classification: {Name} (Exists: {Exists})", classificationName, exists);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence for classification: {Name}", classificationName);
            throw;
        }
    }

    public async Task<OperationResult<ClassificationDto>> UpdateClassificationAsync(int classificationId, string classificationName)
    {
        try
        {
            var result = await _classificationRepository.UpdateClassificationAsync(classificationId, classificationName);
            _logger.LogInformation("Updated classification: {Id} to {Name} (Success: {Success})", classificationId, classificationName, result.IsSuccess);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating classification: {Id} to {Name}", classificationId, classificationName);
            throw;
        }
    }
}