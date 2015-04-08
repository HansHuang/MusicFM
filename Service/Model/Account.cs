using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Model
{
    [Serializable]
    public class Account
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public DateTime LoginTime { get; set; }
        public string ExpireString { get; set; }
        public DateTime? Expire { get; set; }
        //Get user account from webpage
        public string Cookie { get; set; }
        public string BdUss { get; set; }
        public string Ptoken { get; set; }
        public string Stoken { get; set; }

        public string R { get; set; }

        public AccountType AccountType { get; set; }
    }

    [Serializable]
    public enum AccountType
    {
        DoubanFm = 1,
        Weibo = 2,
        TencentQq = 6,
        Msn = 5,
        Baidu = 100
    }
}
