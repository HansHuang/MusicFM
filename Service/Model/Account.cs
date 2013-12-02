using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Model
{
    public class Account
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public DateTime LoginTime { get; set; }
        public string ExpireString { get; set; }
        public DateTime Expire { get; set; }

        public string R { get; set; }
    }
}
