using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class Collection : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid UserId { get; set; }
        public bool IsPublic { get; set; }
        public int PublishedPoemCount { get; set; }
        public ICollection<SavedPoem> SavedPoems { get; set; } = new List<SavedPoem>();
    }

}
