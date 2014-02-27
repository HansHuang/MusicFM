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
        protected Random Randomer;

        public DoubanFm()
        {
            Randomer = new Random(1000000);
        }

        public List<Song> GetSongList(GetSongParameter param) 
        {
            if (param == null) return new List<Song>();
            var url = new StringBuilder(string.Format("http://douban.fm/j/app/radio/people?app_name=radio_desktop_win&version=100"));
            url.Append("&channel=" + param.ChannelId);
            url.Append("&sid=" + (string.IsNullOrWhiteSpace(param.SongId) ? "0" : param.SongId));
            if (!string.IsNullOrWhiteSpace(param.UserId)) url.Append("&user_id=" + param.UserId);
            if (!string.IsNullOrEmpty(param.Expire)) url.Append("&expire=" + param.Expire);
            if (!string.IsNullOrEmpty(param.Token)) url.Append("&token=" + param.Token);
            if (!string.IsNullOrEmpty(param.History)) url.Append("&h=" + param.History);


            var type = "p";
            if (!string.IsNullOrEmpty(param.History))
            {
                if (param.History.Contains(":r")) type = "r"; //Like song(red song)
                else if (param.History.Contains(":u")) type = "u"; //Unlike song
                else if (param.History.Contains(":s")) type = "b"; //Hate song
            }
            url.Append("&type=" + type);
            url.Append("&r=" + Randomer.Next(0, 1000000));

            var json = HttpWebDealer.GetJsonObject(url.ToString(), Encoding.UTF8);
            if (json == null || json["song"] == null) return GetSongList(param);
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
                new Channel(0, "私人"),
                new Channel(-3, "红心"),
                new Channel(1, "华语"),
                new Channel(2, "欧美"),
                new Channel(3, "70"),
                new Channel(4, "80"),
                new Channel(5, "90"),
                new Channel(7, "摇滚"),
                new Channel(8, "民谣"),
                new Channel(9, "轻音乐"),
                new Channel(10, "电影原声"),
                new Channel(13, "爵士,"),
                new Channel(14, "电子"),
                new Channel(15, "说唱"),
                new Channel(16, "R&B"),
                new Channel(17, "日语"),
                new Channel(18, "韩语"),
                new Channel(20, "女声"),
                new Channel(22, "法语")
            };
            if (isBasic) return list;

            //Get expansion channels
            var json = HttpWebDealer.GetJsonObject(
                "http://www.douban.com/j/app/radio/channels?version=100&app_name=radio_desktop_win", Encoding.UTF8);
            if (json == null || json["channels"] == null) return GetChannelsFromWebpage(list);

            foreach (var element in json["channels"]) 
            {
                var cid = Convert.ToInt32(element["channel_id"]);
                if (list.Any(s => s.Id == cid)) continue;
                list.Add(new Channel(cid, element["name"]));
            }

            return list;
        }

        /// <summary>
        /// After Completed song, send info to server
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CompletedSong(GetSongParameter parameter)
        {
            if (parameter == null) return false;
            var url = new StringBuilder(string.Format("http://douban.fm/j/app/radio/people?app_name=radio_desktop_win&version=100&type=e"));
            url.Append("&user_id=" + parameter.UserId);
            url.Append("&expire=" + parameter.Expire);
            url.Append("&token=" + parameter.Token);
            url.Append("&sid=" + parameter.SongId);
            url.Append("&channel=" + parameter.ChannelId);

            var json = HttpWebDealer.GetJsonObject(url.ToString(), Encoding.UTF8);

            return json != null && json["r"] == 0;
        }

        /// <summary>
        /// Get douban fm channels by browsering webpage then select channels from html document
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private ObservableCollection<Channel> GetChannelsFromWebpage(ObservableCollection<Channel> list)
        {
            if (list == null) list = new ObservableCollection<Channel>();
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

        /// <summary>
        /// Login douban fm server
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public Account Login(string userName, string password, AccountType type)
        {
            switch (type)
            {
                case AccountType.DoubanFm:
                    return LoginByDoubanAccount(userName, password);
            }
            return null;
        }

        private Account LoginByDoubanAccount(string userName, string password)
        {
            var json = HttpWebDealer.GetJsonObject("https://www.douban.com/j/app/login?email=" + userName +
                                                          "&password=" + password +
                                                          "&app_name=radio_desktop_win&version=100");
            if (json == null || json["err"] == null) return null;
            var expire = DateTime.Now.AddMilliseconds(Convert.ToInt64(json["expire"]));
            var account = new Account
            {
                Email = json["email"],
                Expire = expire,
                ExpireString = json["expire"],
                LoginTime = DateTime.Now,
                Password = password,
                R = json["r"].ToString(),
                Token = json["token"],
                UserId = json["user_id"].ToString(),
                UserName = json["user_name"],
                AccountType = AccountType.DoubanFm
            };
            return account;
        }

    }
}
