using Anekdotify.Models.DTOs.Classification;
using Anekdotify.Models.Entities;

namespace Anekdotify.BL.Mappers
{
    public static class ClassificationMapper
    {
        public static ClassificationDto ToDto(this Classification classification)
        {
            return new ClassificationDto
            {
                ClassificationName = classification.Name
            };
        }
        public static ClassificationDetailedDto ToDetailedDto(this Classification classification)
        {
            return new ClassificationDetailedDto
            {
                ClassificationId = classification.ClassificationId,
                Name = classification.Name
            };
        }
    }
}