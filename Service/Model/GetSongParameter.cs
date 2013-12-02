using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Model
{
    public class GetSongParameter
    {
        public string UserId { get; set; }
        public string Expire { get; set; }
        public string Token { get; set; }
        public string History { get; set; }
        public int ChannelId { get; set; }
    }
}
