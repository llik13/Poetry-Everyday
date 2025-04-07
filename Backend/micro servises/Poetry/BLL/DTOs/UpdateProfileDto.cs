using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.User.DTOs
{
    public class UpdateProfileDto
    {
        [StringLength(50, MinimumLength = 3)]
        public string UserName { get; set; }

        [StringLength(500)]
        public string Biography { get; set; }
    }

}
