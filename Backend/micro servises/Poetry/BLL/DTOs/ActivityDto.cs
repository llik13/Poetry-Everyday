using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.User.DTOs
{
    public class ActivityDto
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string RelatedId { get; set; }
        public string RelatedTitle { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
