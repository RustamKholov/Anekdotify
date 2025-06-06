using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace api.Helpers
{
    public class JokesQueryObject
    {

        public DateTime? AddingDay { get; set; } = null;
        public string SortBy { get; set; } = string.Empty;
        public bool ByDescending { get; set; } = false;

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        
    }
}