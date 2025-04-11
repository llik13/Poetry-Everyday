using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs
{
    public class PoemSearchDto
    {
        public string? SearchTerm { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
        public List<string> Categories { get; set; } = new List<string>();
        public Guid? AuthorId { get; set; } // Keep this but make it nullable
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10; // Default page size
        public string SortBy { get; set; } = "CreatedAt"; // Default sort field
        public bool SortDescending { get; set; } = true; // Default sort direction
        public bool IsPublished { get; set; } = true; // Changed default to true
    }
}
