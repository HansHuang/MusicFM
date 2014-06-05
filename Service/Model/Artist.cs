using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Model
{
    [Serializable]
    public class Artist
    {
        public int Id { get; set; }
        public string Uid { get; set; }
        public string Name { get; set; }
        public string Region { get; set; }
        public int AlbumCount { get; set; }
        public int SongCount { get; set; }
        public string AvatarUrl { get; set; }
        public string AvatarThumb { get; set; }
    }
}
