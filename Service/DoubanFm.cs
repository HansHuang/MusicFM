using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonHelperLibrary.WEB;
using Service.Model;

namespace Service
{
    /// <summary>
    /// Author : Hans Huang
    /// Date : 2013-10-25
    /// Class : DoubanFm
    /// Discription : Implement of ISongService
    /// </summary>
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
            var url =string.Format("http://douban.fm/j/app/radio/people?app_name=radio_desktop_win&version=100&channel=0&type=p&r={0}&sid=0",
                    _random.Next(0, 1000000));
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



        private Dictionary<int, string> _channels; 
        public Dictionary<int, string> Channels
        {
            get { return _channels ?? (_channels = GetChannels()); }
        }

        private Dictionary<int, string> GetChannels()
        {
            var channels = new Dictionary<int, string>
                {
                    {0, "私人MHz"},
                    {-3, "红心MHz"},
                    {1, "华语MHz"},
                    {2, "欧美MHz"},
                    {3, "70MHz"},
                    {4, "80MHz"},
                    {5, "90MHz"},
                    {7, "摇滚MHz"},
                    {8, "民谣MHz"},
                    {9, "轻音乐MHz"},
                    {10, "电影原声MHz"},
                    {13, "爵士MHz,"},
                    {14, "电子MHz"},
                    {15, "说唱MHz"},
                    {16, "R&BMHz"},
                    {17, "日语MHz"},
                    {18, "韩语MHz"},
                    {20, "女声MHz"},
                    {22, "法语MHz"}
                };
            return channels;
        } 
    }
}
