using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using CommonHelperLibrary;
using CommonHelperLibrary.WEB;
using Service.Model;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace Service
{
    /// <summary>
    /// Author : Hans Huang
    /// Date : 2013-10-25
    /// Class : DoubanFm
    /// Discription : Implement of ISongService
    /// </summary>
    [Export(typeof(ISongService))]
    public class DoubanFm : ISongService
    {
        //Random for get song list
        protected Random Randomer;

        protected LoggerHelper Log;

        public string Name { get { return "DoubanFm"; } }

        public DoubanFm()
        {
            Randomer = new Random(1000000);
            Log = LoggerHelper.Instance;
        }

        public List<Song> GetSongList(GainSongParameter param) 
        {
            var url = BulidUrlForGainSongs(param);
            if (string.IsNullOrWhiteSpace(url)) return new List<Song>();

            WebHeaderCollection header = null;
            if (!string.IsNullOrWhiteSpace(param.AccountCookie))
                header = new WebHeaderCollection {{"Cookie", param.AccountCookie}};
            var json = HttpWebDealer.GetJsonObject(url, header, Encoding.UTF8) as Dictionary<string, object>;
            if (json == null || !json.ContainsKey("song"))
            {
                //TODO: Can't connect to internet
                return new List<Song>();
            }
            var songs = json["song"] as IEnumerable;
            if (songs == null) return new List<Song>();
            //This list will always appear at first time, almost 100% probability
            var count = 1;
            var filterList = new List<string> { "107686", "280187", "1000411", "1027380", "1381349" };
            var list = new List<Song>();
            foreach (dynamic song in songs)
            {
                try
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
                        Thumb=song["picture"],
                        Picture = song["picture"].Replace("mpic", "lpic"),
                        Url = song["url"],
                        Sid = Convert.ToInt32(song["sid"]),
                        Like = Convert.ToInt32(song["like"])
                    });
                }
                catch (Exception e)
                {
                    LoggerHelper.Instance.Exception(e);
                }
            }
            if (count >= 3 || list.Count < 1) return GetSongList(param);
            return list;
        }

        private string BulidUrlForGainSongs(GainSongParameter para) 
        {
            if (para == null) return string.Empty;
            var isFromWebsite = !string.IsNullOrWhiteSpace(para.AccountCookie);
            var url = new StringBuilder();
            if (isFromWebsite)
            {
                url.Append("http://douban.fm/j/mine/playlist?pb=64&from=mainsite");
                if (!string.IsNullOrWhiteSpace(para.Position)) url.Append("&pt=" + para.Position);
            }
            else
            {
                url.Append("http://douban.fm/j/app/radio/people?app_name=radio_desktop_win&version=100");
                if (!string.IsNullOrWhiteSpace(para.UserId)) url.Append("&user_id=" + para.UserId);
                if (!string.IsNullOrEmpty(para.Expire)) url.Append("&expire=" + para.Expire);
                if (!string.IsNullOrEmpty(para.Token)) url.Append("&token=" + para.Token);
                if (!string.IsNullOrEmpty(para.History)) url.Append("&h=" + para.History);
            }
            url.Append("&channel=" + para.Channel.Id);
            url.Append("&sid=" + (string.IsNullOrWhiteSpace(para.SongId) ? "0" : para.SongId));

            //Next song
            var type = isFromWebsite ? "s" : "p";
            switch (para.OperationType)
            {
                case OperationType.Like:
                    type = "r";
                    break;
                case OperationType.DisLike:
                    type = "u";
                    break;
                case OperationType.Hate:
                    type = "b";
                    break;
            }
            url.Append("&type=" + type);
            url.Append("&r=" + Randomer.Next(0, 1000000));

            return url.ToString();
        }

        /// <summary>
        /// Get Music Channels
        /// </summary>
        /// <param name="isBasic">Basic channels or entire channel list</param>
        /// <returns></returns>
        public List<Channel> GetChannels(bool isBasic = true)
        {
            var list = new List<Channel> 
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
                new Channel(13, "爵士"),
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
                "http://www.douban.com/j/app/radio/channels?version=100&app_name=radio_desktop_win", new WebHeaderCollection(), Encoding.UTF8);
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
        public bool CompletedSong(SongActionParameter parameter)
        {
            if (parameter == null) return false;
            var isFromWebsite = !string.IsNullOrWhiteSpace(parameter.AccountCookie);
            var url = new StringBuilder();
            if (isFromWebsite)
            {
                url.Append("http://douban.fm/j/mine/playlist?pb=64&from=mainsite&type=e");
                if (!string.IsNullOrWhiteSpace(parameter.Position)) url.Append("&pt=" + parameter.Position);
                url.Append("&channel=" + parameter.Channel.Id);
                url.Append("&sid=" + (string.IsNullOrWhiteSpace(parameter.SongId) ? "0" : parameter.SongId));
                url.Append("&r=" + Randomer.Next(0, 1000000));
            }
            else
            {
                url.Append("http://douban.fm/j/app/radio/people?app_name=radio_desktop_win&version=100&type=e");
                url.Append("&user_id=" + parameter.UserId);
                url.Append("&expire=" + parameter.Expire);
                url.Append("&token=" + parameter.Token);
                url.Append("&sid=" + parameter.SongId);
                url.Append("&channel=" + parameter.Channel.Id);
            }
            WebHeaderCollection header = null;
            if (isFromWebsite) header = new WebHeaderCollection {{"Cookie", parameter.AccountCookie}};
            var json = HttpWebDealer.GetJsonObject(url.ToString(), header, Encoding.UTF8);

            return json != null && json["r"] == 0;
        }

        /// <summary>
        /// Get douban fm channels by browsering webpage then select channels from html document
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<Channel> GetChannelsFromWebpage(List<Channel> list)
        {
            if (list == null) list = new List<Channel>();
            var html = HttpWebDealer.GetHtml("http://douban.fm", null, Encoding.UTF8);
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
        /// Get Song Lyric
        /// </summary>
        /// <param name="song"></param>
        /// <returns></returns>
        public SongLyric GetLyric(Song song)
        {
            if (song == null) return null;
            var lrc = SongLyricHelper.GetSongLyric(song.Title, song.Artist);
            return lrc;
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
                default:
                    return LoginByThirdPartyAccount(userName, password, type);
            }
        }

        public SearchResult Search(string keyword, int count)
        {
            var result = new SearchResult { Query = keyword };
            var url = string.Format("http://douban.fm/j/explore/search?query={0}&limit={1}&start=0",
                                    HttpUtility.UrlEncode(keyword), count);
            var json = HttpWebDealer.GetJsonObject(url, new WebHeaderCollection(), Encoding.UTF8);
            try
            {
                if (!json["status"]) return result;
                foreach (var channel in json["data"]["channels"])
                {
                    result.ChannelList.Add(new Channel
                    {
                        Id = channel["id"],
                        Name = channel["name"],
                        Description = channel["intro"],
                        CoverImage = channel["banner"],
                        Thumb = channel["cover"]
                    });
                }
                result.CurrentNr = result.ChannelList.Count;
                result.ResultCount = result.ChannelList.Count * (int)json["data"]["total"];
                //Wanted count greater then actual result count
                if (count > result.CurrentNr)
                    result.ResultCount = result.CurrentNr;
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return result;
        }

        public List<Song> GetSongList(Artist artist)
        {
            //not support
            return new List<Song>();
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

        private Account LoginByThirdPartyAccount(string userName, string password, AccountType type) 
        {
            var timeOut = 2000;//2s
            Account account = null;
            var loginUrl = "http://douban.fm/partner/login?target=" + (int)type;
            var trd = new Thread(() =>
            {
                var browser = new WebBrowser { ScriptErrorsSuppressed = true };
                browser.DocumentCompleted += (sdr, e) =>
                {
                    if (browser.Document.Url.Host.Contains("api.weibo.com"))
                    {
                        browser.Document.GetElementById("userId").SetAttribute("value", userName);
                        //Browser.Document.GetElementById("passwd").Focus();
                        browser.Document.GetElementById("passwd").SetAttribute("value", password);
                        var submitBtn = browser.Document.GetElementsByTagName("a")
                                               .OfType<HtmlElement>()
                                               .FirstOrDefault(s => s.GetAttribute("action-type").Equals("submit"));
                        if (submitBtn != null)
                        {
                            submitBtn.InvokeMember("click");
                            timeOut = 3000;
                        }
                        return;
                    }

                    var name = string.Empty;
                    var nameBox = browser.Document.GetElementById("user_name");
                    if (nameBox != null) name = nameBox.InnerText;

                    var cookie = IeCookieHelper.GetCookieData("http://douban.fm");
                    if (cookie.Contains("dbcl2"))
                    {
                        var cookieList = cookie.Split(' ').Where(s => !s.Contains("_")).ToList();
                        var userId = cookieList.First(s => s.Contains("dbcl2"))
                                               .Split(new[] { '=', '"', ':' }, StringSplitOptions.RemoveEmptyEntries)[1];
                        cookie = cookieList.Aggregate("", (seed, ele) => seed + ele + " ");
                        account = new Account
                        {
                            AccountType = type,
                            Email = userName,
                            UserName = name,
                            Password = password,
                            UserId = userId,
                            Cookie = cookie,
                            LoginTime = DateTime.Now,
                            Expire = DateTime.Now.AddDays(5)
                        };
                        browser.Dispose();
                        Application.ExitThread();
                        return;
                    }
                };
                browser.Navigate(loginUrl);
                Application.Run();
            }) { IsBackground = true };
            trd.SetApartmentState(ApartmentState.STA);
            trd.Start();
            //trd.Join();

            while (true)
            {
                if (timeOut < 1 || account != null) break;
                timeOut -= 100;
                Thread.Sleep(100);
            }
            return account;
        }


    }
}
