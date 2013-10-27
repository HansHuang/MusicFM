using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonHelperLibrary.WEB;

namespace Service.Model
{
    public class DoubanFm : ISongService 
    {
        //Random for get song list
        private Random _random;

        public DoubanFm() 
        {
            _random = new Random(1000000);
        }

        public List<Song> GetSongList()
        {
            var url = string.Format("http://douban.fm/j/app/radio/people?app_name=radio_desktop_win&version=100&channel=0&type=p&r={0}&sid=0",
                    _random.Next());
            var json = HttpWebDealer.GetJsonObject(url, Encoding.UTF8);
            var songs = json["song"] as IEnumerable;
            if (songs == null) return new List<Song>();
            var list = (from dynamic song in songs
                        select new Song
                            {
                                Title = song["title"],
                                Artist = song["artist"],
                                AlbumTitle = song["albumtitle"],
                                AlbumId = song["album"],
                                Company = song["company"],
                                PublishTime = song["public_time"],
                                Length = Convert.ToInt32(song["length"]),
                                Kbps = Convert.ToInt32(song["kbps"]),
                                Picture = song["picture"],
                                Url = song["url"],
                                Sid = Convert.ToInt32(song["sid"]),
                                Like = Convert.ToInt32(song["like"])
                            }).Where(s => s.Length > 15).ToList();
            //fitle advertisement
            list.ForEach(s => s.Picture = s.Picture.Replace("mpic", "lpic"));
            return list;
        }


    }
}
