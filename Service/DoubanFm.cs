using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
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

        /// <summary>
        /// Get Music Channels
        /// </summary>
        /// <param name="isBasic">Basic channels or entire channel list</param>
        /// <returns></returns>
        public ObservableCollection<Channel> GetChannels(bool isBasic = true) {
            var basic = new ObservableCollection<Channel> {
                new Channel(0, "私人MHz"),
                new Channel(-3, "红心MHz"),
                new Channel(1, "华语MHz"),
                new Channel(2, "欧美MHz"),
                new Channel(3, "70MHz"),
                new Channel(4, "80MHz"),
                new Channel(5, "90MHz"),
                new Channel(7, "摇滚MHz"),
                new Channel(8, "民谣MHz"),
                new Channel(9, "轻音乐MHz"),
                new Channel(10, "电影原声MHz"),
                new Channel(13, "爵士MHz,"),
                new Channel(14, "电子MHz"),
                new Channel(15, "说唱MHz"),
                new Channel(16, "R&BMHz"),
                new Channel(17, "日语MHz"),
                new Channel(18, "韩语MHz"),
                new Channel(20, "女声MHz"),
                new Channel(22, "法语MHz")
            };
            if (isBasic) return basic;

            //Get expansion channels
            var xd = XDocument.Load(HttpWebDealer.GetHtml("http://douban.fm"));
            
            return null;
        } 
    }
}
