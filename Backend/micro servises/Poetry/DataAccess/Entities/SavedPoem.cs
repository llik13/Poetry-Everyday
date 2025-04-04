using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class SavedPoem : BaseEntity
    {
        public Guid CollectionId { get; set; }
        public Guid PoemId { get; set; }
        public DateTime SavedAt { get; set; }
        public Collection Collection { get; set; }
    }

}
