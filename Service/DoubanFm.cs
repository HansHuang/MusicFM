using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonHelperLibrary.WEB;
using HtmlAgilityPack;
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

        public List<Song> GetSongList(GetSongParameter param) 
        {
            if (param == null) return new List<Song>();
            var url = new StringBuilder(string.Format("http://douban.fm/j/app/radio/people?app_name=radio_desktop_win&version=100&channel={0}&type=p&r={1}&sid=0",
                        param.ChannelId, _random.Next(0, 1000000)));
            if (!string.IsNullOrWhiteSpace(param.UserId))
                url.Append("&user_id=" + param.UserId);
            if(!string.IsNullOrEmpty(param.Expire))
                url.Append("&expire=" + param.Expire);
            if (!string.IsNullOrEmpty(param.Token))
                url.Append("&token=" + param.Token);
            if (!string.IsNullOrEmpty(param.History))
                url.Append("&h=" + param.History);

            var json = HttpWebDealer.GetJsonObject(url.ToString(), Encoding.UTF8);
            if (json == null) return GetSongList(param);
            var songs = json["song"] as IEnumerable;
            //This list will always appear at first time, almost 100% probability
            var count = 1;
            var filterList = new List<string>{"107686","280187","1000411","1027380","1381349"};
            var list = new List<Song>();
            foreach (dynamic song in songs)
            {
                //filter advertisement
                var length = Convert.ToInt32(song["length"]);
                if (length <= 30) continue;
                if (filterList.Contains(song["sid"])) count++;
                list.Add(new Song
                {
                    Title = song["title"],
                    Artist = song["artist"],
                    AlbumTitle = song["albumtitle"],
                    AlbumId = song["album"],
                    Company = song["company"],
                    PublishTime = song["public_time"],
                    Length = length,
                    Kbps = Convert.ToInt32(song["kbps"]),
                    Picture = song["picture"].Replace("mpic", "lpic"),
                    Url = song["url"],
                    Sid = Convert.ToInt32(song["sid"]),
                    Like = Convert.ToInt32(song["like"])
                });
            }
            if (count >= 3 || list.Count < 1) return GetSongList(param);
            return list;
        }

        /// <summary>
        /// Get Music Channels
        /// </summary>
        /// <param name="isBasic">Basic channels or entire channel list</param>
        /// <returns></returns>
        public ObservableCollection<Channel> GetChannels(bool isBasic = true)
        {
            var list = new ObservableCollection<Channel> 
            {
                new Channel(0, "私人 MHz"),
                new Channel(-3, "红心 MHz"),
                new Channel(1, "华语 MHz"),
                new Channel(2, "欧美 MHz"),
                new Channel(3, "70 MHz"),
                new Channel(4, "80 MHz"),
                new Channel(5, "90 MHz"),
                new Channel(7, "摇滚 MHz"),
                new Channel(8, "民谣 MHz"),
                new Channel(9, "轻音乐 MHz"),
                new Channel(10, "电影原声 MHz"),
                new Channel(13, "爵士 MHz,"),
                new Channel(14, "电子 MHz"),
                new Channel(15, "说唱 MHz"),
                new Channel(16, "R&B MHz"),
                new Channel(17, "日语 MHz"),
                new Channel(18, "韩语 MHz"),
                new Channel(20, "女声 MHz"),
                new Channel(22, "法语 MHz")
            };
            if (isBasic) return list;

            //Get expansion channels
            var html = HttpWebDealer.GetHtml("http://douban.fm", Encoding.UTF8);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var chls = doc.DocumentNode.SelectNodes("//ul[@id=\"promotion_chls\"]");
            if (chls == null) return list;
            foreach (var node in chls.Nodes().Where(s => !string.IsNullOrWhiteSpace(s.InnerHtml)))
            {
                var cid = node.GetAttributeValue("cid", 0);
                if (list.Any(s => s.Id == cid)) continue;
                var desc = node.GetAttributeValue("data-intro", "");
                var conver = node.GetAttributeValue("data-cover", "");
                var name = node.ChildNodes.First(s => !string.IsNullOrWhiteSpace(s.InnerHtml)).InnerText;

                list.Add(new Channel(cid, name, desc, conver));
            }

            return list;
        }

    }
}
