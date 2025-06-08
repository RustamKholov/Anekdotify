using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTOs.Classification
{
    public class ClassificationDetailedDTO
    {
        public int ClassificationId { get; set; }
        public string Name { get; set; } = null!;
    }
}