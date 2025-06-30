using Anekdotify.BL.Interfaces.Repositories;
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
        public async Task<OperationResult<Classification>> CreateClassificationAsync(string classificationName)
        {
            var classification = new Classification { Name = classificationName };
            var newClassification = await _context.Classifications.AddAsync(classification);
            await _context.SaveChangesAsync();
            return OperationResult<Classification>.Success(newClassification.Entity);

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

        public async Task<OperationResult<List<ClassificationDetailedDto>>> GetAllClassificationsAsync()
        {
            var classifications = await _context.Classifications.Select(cl => cl.ToDetailedDto()).ToListAsync();
            return OperationResult<List<ClassificationDetailedDto>>.Success(classifications);
        }

        public async Task<OperationResult<ClassificationDto>> GetClassificationByIdAsync(int classificationId)
        {
            var existingClassification = await _context.Classifications.FirstOrDefaultAsync(cl => cl.ClassificationId == classificationId);
            if (existingClassification == null)
            {
                return OperationResult<ClassificationDto>.NotFound(new ClassificationDto { ClassificationName = "NotFound" }, "Classification not found");
            }
            return OperationResult<ClassificationDto>.Success(existingClassification.ToDto());
        }

        public async Task<OperationResult<Classification>> GetClassificationByNameAsync(string classificationName)
        {
            var existingClassification = await _context.Classifications.FirstOrDefaultAsync(cl => cl.Name.ToLower() == classificationName.ToLower());
            if (existingClassification == null)
            {
                return OperationResult<Classification>.NotFound(new Classification { Name = "NotFound" }, "Classification not found");
            }
            return OperationResult<Classification>.Success(existingClassification);
        }

        public async Task<bool> IsExistingAsync(string classificationName)
        {
            return await _context.Classifications.AnyAsync(cl => cl.Name.ToLower() == classificationName.ToLower());
        }

        public async Task<OperationResult<ClassificationDto>> UpdateClassificationAsync(int classificationId, string classificationName)
        {
            var existingClassification = await _context.Classifications.FirstOrDefaultAsync(cl => cl.ClassificationId == classificationId);
            if (existingClassification == null)
            {
                return OperationResult<ClassificationDto>.NotFound(new ClassificationDto { ClassificationName = "NotFound" }, "Classification not found");
            }
            existingClassification.Name = classificationName;
            await _context.SaveChangesAsync();
            return OperationResult<ClassificationDto>.Success(existingClassification.ToDto());
        }
    }
}