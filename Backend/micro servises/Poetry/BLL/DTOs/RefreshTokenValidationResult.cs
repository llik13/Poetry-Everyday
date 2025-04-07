using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.User.DTOs
{
    public class RefreshTokenValidationResult
    {
        public bool IsValid { get; set; }
        public string UserId { get; set; }
    }

}
