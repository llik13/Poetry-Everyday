using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class Like : BaseEntity
    {
        public Guid PoemId { get; set; }
        public Guid UserId { get; set; }
        public Poem Poem { get; set; }
    }

}
