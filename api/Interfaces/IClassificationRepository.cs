using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Common;
using api.DTOs.Classification;
using api.Models;

namespace api.Interfaces
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