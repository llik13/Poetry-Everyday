using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class PoemVersion : BaseEntity
    {
        public Guid PoemId { get; set; }
        public string Content { get; set; } // Full version content stored directly in DB
        public int VersionNumber { get; set; }
        public string ChangeNotes { get; set; }
        public Poem Poem { get; set; }
    }

}
