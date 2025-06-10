using Anekdotify.Models.DTOs.Classification;
using Anekdotify.Models.Entities;

namespace Anekdotify.BL.Mappers
{
    public static class ClassificationMapper
    {
        public static ClassificationDTO ToDTO(this Classification classification)
        {
            return new ClassificationDTO
            {
                ClassificationName = classification.Name
            };
        }
        public static ClassificationDetailedDTO ToDetailedDTO(this Classification classification)
        {
            return new ClassificationDetailedDTO
            {
                ClassificationId = classification.ClassificationId,
                Name = classification.Name
            };
        }
    }
}