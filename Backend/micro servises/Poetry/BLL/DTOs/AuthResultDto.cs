using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.User.DTOs
{
    public class AuthResultDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int ExpiresIn { get; set; } // Token expiration in seconds
    }

}
