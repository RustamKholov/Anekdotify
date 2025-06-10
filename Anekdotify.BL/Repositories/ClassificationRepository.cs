using Anekdotify.BL.Interfaces;
using Anekdotify.BL.Mappers;
using Anekdotify.Common;
using Anekdotify.Database.Data;
using Anekdotify.Models.DTOs.Classification;
using Anekdotify.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Anekdotify.BL.Repositories
{
    public class ClassificationRepository : IClassificationRepository
    {
        private readonly ApplicationDBContext _context;
        public ClassificationRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task<OperationResult<ClassificationDTO>> CreateClassificationAsync(string classificationName)
        {
            var classification = new Classification { Name = classificationName };
            await _context.Classifications.AddAsync(classification);
            await _context.SaveChangesAsync();
            return OperationResult<ClassificationDTO>.Success(classification.ToDTO());

        }

        public async Task<OperationResult> DeleteClassificationAsync(int classificationId)
        {
            var existingClassification = await _context.Classifications.FirstOrDefaultAsync(cl => cl.ClassificationId == classificationId);
            if (existingClassification == null)
            {
                return OperationResult.NotFound("Classification not found");
            }
            _context.Classifications.Remove(existingClassification);
            await _context.SaveChangesAsync();
            return OperationResult.Success();
        }

        public async Task<OperationResult<List<ClassificationDetailedDTO>>> GetAllClassificationsAsync()
        {
            var classifications = await _context.Classifications.Select(cl => cl.ToDetailedDTO()).ToListAsync();
            return OperationResult<List<ClassificationDetailedDTO>>.Success(classifications);
        }

        public async Task<OperationResult<ClassificationDTO>> GetClassificationByIdAsync(int classificationId)
        {
            var existingClassification = await _context.Classifications.FirstOrDefaultAsync(cl => cl.ClassificationId == classificationId);
            if (existingClassification == null)
            {
                return OperationResult<ClassificationDTO>.NotFound(new ClassificationDTO { ClassificationName = "NotFound" }, "Classification not found");
            }
            return OperationResult<ClassificationDTO>.Success(existingClassification.ToDTO());
        }

        public async Task<bool> IsExistingAsync(string classificationName)
        {
            return await _context.Classifications.AnyAsync(cl => cl.Name == classificationName.ToLower());
        }

        public async Task<OperationResult<ClassificationDTO>> UpdateClassificationAsync(int classificationId, string classificationName)
        {
            var existingClassification = await _context.Classifications.FirstOrDefaultAsync(cl => cl.ClassificationId == classificationId);
            if (existingClassification == null)
            {
                return OperationResult<ClassificationDTO>.NotFound(new ClassificationDTO { ClassificationName = "NotFound" }, "Classification not found");
            }
            existingClassification.Name = classificationName;
            await _context.SaveChangesAsync();
            return OperationResult<ClassificationDTO>.Success(existingClassification.ToDTO());
        }
    }
}