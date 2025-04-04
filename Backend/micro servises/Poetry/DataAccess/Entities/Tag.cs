using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class Tag : BaseEntity
    {
        public string Name { get; set; }
        public ICollection<Poem> Poems { get; set; } = new List<Poem>();
    }

}
