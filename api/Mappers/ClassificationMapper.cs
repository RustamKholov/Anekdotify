using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using api.DTOs.Classification;
using api.Models;

namespace api.Mappers
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