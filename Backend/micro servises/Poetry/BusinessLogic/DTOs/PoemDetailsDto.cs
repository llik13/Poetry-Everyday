using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs
{
    public class PoemDetailsDto : PoemDto
    {
        public List<CommentDto> Comments { get; set; } = new List<CommentDto>();
        public bool IsLikedByCurrentUser { get; set; }
    }

}
