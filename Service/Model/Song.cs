using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Model
{
    [Serializable]
    public class Song
    {
        public int Sid { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string AlbumTitle { get; set; }
        public string AlbumId { get; set; }
        public string Company { get; set; }
        public string PublishTime { get; set; }
        public int Length { get; set; }
        public int Kbps { get; set; }
        public string Url { get; set; }
        public string LrcUrl { get; set; }
        public string Picture { get; set; }
        public string Thumb { get; set; }
        public int Like { get; set; }

    }
}
