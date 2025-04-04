using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs
{
    public class CollectionDetailsDto : CollectionDto
    {
        public List<PoemDto> Poems { get; set; } = new List<PoemDto>();
    }

}
