using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class PoemStatistics : BaseEntity
    {
        public Guid PoemId { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public int SaveCount { get; set; }
        public Poem Poem { get; set; }
    }

}
