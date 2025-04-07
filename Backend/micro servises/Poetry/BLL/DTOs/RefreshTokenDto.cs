using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.User.DTOs
{
    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; }
    }

}
