using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs
{
    public class PoemVersionDto
    {
        public Guid Id { get; set; }
        public int VersionNumber { get; set; }
        public string ChangeNotes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
