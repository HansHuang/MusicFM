using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvPlayer.Service.Model
{
    public class MusicVideo
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public string PlayPageUrl { get; set; }
        public string FlvUrl { get; set; }
        public List<MvArtist> Artists { get; set; }

        public MusicVideo()
        {
            Artists = new List<MvArtist>();
        }
    }

    /// <summary>
    /// The Artist or music video
    /// </summary>
    public class MvArtist
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MainPage { get; set; }
    }
}
