using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CommonHelperLibrary;
using CommonHelperLibrary.WEB;
using Service.Model;

namespace Service
{
    /// <summary>
    /// Author : Hans Huang
    /// Date : 2014-04-13
    /// Class : BaiduMusic
    /// Discription : Implement of ISongService with baidu music service
    /// </summary>
    [Export(typeof(ISongService))]
    public class BaiduMusic : ISongService 
    {
        private List<Channel> _basicChannels;
        private readonly Dictionary<string, int> _gotSongList = new Dictionary<string, int>();
        private KeyValuePair<int, int> _artistSongList;
        private readonly WebHeaderCollection _headers;
        //private readonly WebClient _webClient;
        private int _getSongsFailed;

        protected string BaseUrl;
        protected LoggerHelper Logger;

        public string Name { get { return "BaiduMusic"; } }

        public string LocalizeName { get; set; }

        public List<AccountType> AvaliableAccountTypes { get; private set; }

        public BaiduMusic()
        {
            BaseUrl = "http://tingapi.ting.baidu.com/v1/restserver/ting?format=json&from=ttpwin8&version=1.0.4";
            Logger = LoggerHelper.Instance;
            AvaliableAccountTypes = new List<AccountType> {AccountType.Baidu};

            _headers = new WebHeaderCollection { { "user-agent", "ttpwin8_1.0.4" } };
            //_webClient = new WebClient {Encoding = Encoding.UTF8};
        }

        public async Task<List<Song>> GetSongList(GainSongParameter param) 
        {
            var songList = new List<Song>();
            if (_getSongsFailed == 0 && !SongFavoriteOperation(param)) return songList;
            if (_getSongsFailed > 3) return songList;

            var url = BulidUrlForGainSongs(param);
            if (string.IsNullOrWhiteSpace(url)) return songList;

            try
            {
                var task = HttpWebDealerAsyc.GetJsonObject(url, _headers, Encoding.UTF8);
                var result = (await task)["result"];
                var songs = (IEnumerable)result["songlist"];
                songList.AddRange(from dynamic song in songs
                                  select new Song
                                  {
                                      Title = song["title"],
                                      Artist = song["artist"],
                                      Sid = Convert.ToInt32(song["songid"]),
                                  });
                if (songList.Count < 1) return await GetSongList(param);
                _gotSongList[param.Channel.StrId]++;
                _getSongsFailed = 0;

                songList.ForEach(GetSongInfo);
                songList = songList.Where(s => !string.IsNullOrWhiteSpace(s.Url)).ToList();
            }
            catch (Exception e)
            {
                Logger.Exception(e);
                _getSongsFailed++;
            }

            return songList;
        }

        public async Task<List<Channel>> GetChannels(bool isBasic = true)
        {
            //if (isBasic)
            if (_basicChannels == null || _basicChannels.Count < 0)
                GetBasicChannels();

            return await Task.Run(() => _basicChannels);
        }

        /// <summary>
        /// Get Song Lyric
        /// </summary>
        /// <param name="song"></param>
        /// <returns></returns>
        public SongLyric GetLyric(Song song)
        {
            if (song == null) return null;
            var lrc = string.IsNullOrWhiteSpace(song.LrcUrl)
                          ? SongLyricHelper.GetSongLyric(song.Title, song.Artist)
                          : SongLyricHelper.GetSongLyric(song.LrcUrl);
            return lrc;
        }

        public async Task<bool> CompletedSong(SongActionParameter parameter)
        {
            return await Task.Run(() => true);
        }

        public async Task<Account> Login(string userName, string password, AccountType type) 
        {
            Account account = null;
            if (type != AccountType.Baidu) return null;
            const string url = "http://passport.baidu.com/v2/sapi/login";
            var content = BulidContentForLogin(userName, password);

            var header = new WebHeaderCollection
            {
                {"user-agent", "ttpwin8_1.0.4"},
                {"ContentType", "application/x-www-form-urlencoded"},
                {"Method","POST"},
                {"PostData",content}
            };
            try
            {
                var json =await HttpWebDealerAsyc.GetJsonObject(url, header, Encoding.UTF8);

                //need verify code
                if (json["needvcode"] == 1)
                {
                    //TODO: need verify code
                }
                else if (json["errno"]==0)
                {
                    account = new Account
                    {
                        AccountType = type,
                        Email=userName,
                        Password=password,
                        LoginTime=DateTime.Now,
                        Expire=DateTime.Now.AddDays(7),
                        UserName = json["displayname"],
                        UserId = json["uid"].ToString(),
                        BdUss = json["bduss"],
                        Stoken = json["stoken"],
                        Ptoken = json["ptoken"]
                    };
                }
            }
            catch (Exception e)
            {
                Logger.Exception(e);
            }
            return account;
        }

        public async Task<SearchResult> Search(string keyword, int count) 
        {
            var url = BaseUrl + string.Format("&method=baidu.ting.search.common&query={0}&page_size={1}&page_no=1",
                                              HttpUtility.UrlEncode(keyword), count);
            var json = await HttpWebDealerAsyc.GetJsonObject(url, _headers, Encoding.UTF8);
            if (json == null) return null;
            var result = await Task.Run(() => GetSearchResultData(json, count));
            return result;
        }

        public async Task<List<Song>> GetSongList(Artist artist) 
        {
            var songList = new List<Song>();
            if (artist == null || artist.Id < 0) return songList;
            if (_artistSongList.Key != artist.Id) _artistSongList = new KeyValuePair<int, int>(artist.Id, 0);
            //Order: 1-time; 2-hot
            var order = DateTime.Now.Millisecond%2;
            var url = BaseUrl + string.Format("&method=baidu.ting.artist.getSongList&artistid={0}&offset={1}&limits=10&order={2}",
                          artist.Id, _artistSongList.Value, order);
            try
            {
                var json = await HttpWebDealerAsyc.GetJsonObject(url, _headers, Encoding.UTF8);
                if (json == null) return null;
                if (json["error_code"].ToString() != BaiduJsonErrorCode.OK)
                {
                    Logger.Msg("Baidu Json Error Code: " + json["error_code"], url);
                    return songList;
                }
                var songs = (IEnumerable)json["songlist"];
                songList.AddRange(from dynamic song in songs
                                  select new Song
                                  {
                                      Title = song["title"],
                                      Artist = song["author"],
                                      Sid = Convert.ToInt32(song["song_id"]),
                                  });

                var offset = json["havemore"] == 1 ? _artistSongList.Value + songList.Count : 0;
                _artistSongList = new KeyValuePair<int, int>(artist.Id, offset);
                //Get detail info of each song
                songList.ForEach(GetSongInfo);
                songList = songList.Where(s => !string.IsNullOrWhiteSpace(s.Url)).ToList();
            }
            catch (Exception e)
            {
                Logger.Exception(e);
            }


            return songList;
        }

        public SearchResult GetSearchResultData(dynamic json, int count) 
        {
            var result = new SearchResult 
            {
                Query = json["query"],
                ResultCount = Convert.ToInt32(json["pages"]["total"]),
                CurrentNr = Convert.ToInt32(json["pages"]["rn_num"])
            };
            if (count > result.CurrentNr)
                result.ResultCount = result.CurrentNr;
            if (json.ContainsKey("artist"))
            {
                try
                {
                    result.Artist = new Artist
                    {
                        Id = Convert.ToInt32(json["artist"]["artist_id"]),
                        Uid = json["artist"]["ting_uid"],
                        Name = json["artist"]["name"],
                        Region = json["artist"]["country"],
                        AlbumCount = Convert.ToInt32(json["artist"]["albums_total"]),
                        SongCount = Convert.ToInt32(json["artist"]["songs_total"]),
                        AvatarUrl = json["artist"]["avatar"]["big"],
                        AvatarThumb = json["artist"]["avatar"]["small"],
                    };
                }
                catch (Exception e)
                {
                    Logger.Exception(e);
                }
            }
            if (json.ContainsKey("song_list"))
            {
                try
                {
                    result.SongList.AddRange(from dynamic song in (IEnumerable)json["song_list"]
                                             select new Song
                                             {
                                                 Title = song["title"].Replace("<em>", "").Replace("</em>", ""),
                                                 Artist = song["author"].Replace("<em>", "").Replace("</em>", ""),
                                                 Sid = Convert.ToInt32(song["song_id"]),
                                             });
                    result.SongList.ForEach(GetSongInfo);
                    result.SongList = result.SongList.Where(s => !string.IsNullOrWhiteSpace(s.Url)).ToList();
                }
                catch (Exception e)
                {
                    Logger.Exception(e);
                }
            }
            return result;
        }

        private void GetBasicChannels()
        {
            _basicChannels = new List<Channel> 
            {
                new Channel
                {
                    StrId = "public_tuijian_suibiantingting", Id = 1, Name = "随便听听",
                    CoverImage = "http://a.hiphotos.baidu.com/ting/pic/item/8694a4c27d1ed21b47c49229ac6eddc450da3fe8.jpg"
                },
                new Channel
                {
                    StrId = "public_shiguang_80hou", Id = 2, Name = "80后",
                    CoverImage = "http://d.hiphotos.baidu.com/ting/pic/item/c75c10385343fbf2688d1befb17eca8064388f5d.jpg"
                },
                new Channel 
                {
                    StrId = "public_shiguang_jingdianlaoge", Id = 3, Name = "经典老歌",
                    CoverImage = "http://b.hiphotos.baidu.com/ting/pic/item/91529822720e0cf361dcf2370b46f21fbf09aad7.jpg"
                },
                new Channel
                {
                    StrId = "public_xinqing_huankuai", Id = 4, Name = "快乐旋律", 
                    CoverImage = "http://a.hiphotos.baidu.com/ting/pic/item/83025aafa40f4bfb92362398024f78f0f6361813.jpg"
                },
                new Channel 
                {
                    StrId = "public_shiguang_xinge", Id = 6, Name = "火爆新歌", 
                    CoverImage = "http://a.hiphotos.baidu.com/ting/pic/item/d833c895d143ad4bb2c6fb0c83025aafa50f0615.jpg"
                },
                new Channel
                {
                    StrId = "public_tuijian_ktv",
                    Id = 45,
                    Name = "KTV金曲",
                    CoverImage = "http://c.hiphotos.baidu.com/ting/pic/item/f3d3572c11dfa9ec2ab2ebdb63d0f703908fc15a.jpg"
                },
                new Channel 
                {
                    StrId = "public_tuijian_billboard",
                    Id = 55,
                    Name = "Billboard",
                    CoverImage = "http://d.hiphotos.baidu.com/ting/pic/item/c9fcc3cec3fdfc03d1c55e74d53f8794a5c226b8.jpg"
                },
                new Channel
                {
                    StrId = "public_tuijian_chengmingqu",
                    Id = 48,
                    Name = "成名曲",
                    CoverImage = "http://b.hiphotos.baidu.com/ting/pic/item/902397dda144ad34bd203a4bd1a20cf430ad855b.jpg"
                },
                new Channel
                {
                    StrId = "public_tuijian_wangluo",
                    Id = 51,
                    Name = "网络红歌",
                    CoverImage = "http://a.hiphotos.baidu.com/ting/pic/item/472309f790529822c659850fd6ca7bcb0b46d490.jpg"
                },
                new Channel
                {
                    StrId = "public_tuijian_kaiche",
                    Id = 42,
                    Name = "开车",
                    CoverImage = "http://a.hiphotos.baidu.com/ting/pic/item/4a36acaf2edda3cc930005ec00e93901203f9212.jpg"
                },
                new Channel
                {
                    StrId = "public_tuijian_yingshi",
                    Id = 57,
                    Name = "影视",
                    CoverImage = "http://b.hiphotos.baidu.com/ting/pic/item/a1ec08fa513d2697f195b49154fbb2fb4216d8bc.jpg"
                },
                new Channel
                {
                    StrId = "public_shiguang_70hou",
                    Id = 43,
                    Name = "70后",
                    CoverImage = "http://c.hiphotos.baidu.com/ting/pic/item/b999a9014c086e068b00828703087bf40bd1cb56.jpg"
                },
                new Channel 
                {
                    StrId = "public_shiguang_90hou",
                    Id = 25,
                    Name = "90后",
                    CoverImage = "http://d.hiphotos.baidu.com/ting/pic/item/77094b36acaf2edd4d51f4258c1001e93801935e.jpg"
                },
                new Channel
                {
                    StrId = "public_shiguang_erge",
                    Id = 24,
                    Name = "儿歌",
                    CoverImage = "http://c.hiphotos.baidu.com/ting/pic/item/242dd42a2834349bc8f4d349c8ea15ce37d3be08.jpg"
                },
                new Channel 
                {
                    StrId = "public_shiguang_lvxing",
                    Id = 16,
                    Name = "旅行",
                    CoverImage = "http://d.hiphotos.baidu.com/ting/pic/item/728da9773912b31b308f70e58718367adbb4e11f.jpg"
                },
                new Channel
                {
                    StrId = "public_shiguang_yedian",
                    Id = 41,
                    Name = "夜店",
                    CoverImage = "http://d.hiphotos.baidu.com/ting/pic/item/3bf33a87e950352a132499c45243fbf2b3118bb1.jpg"
                },
                new Channel 
                {
                    StrId = "public_fengge_liuxing",
                    Id = 46,
                    Name = "流行",
                    CoverImage = "http://c.hiphotos.baidu.com/ting/pic/item/6609c93d70cf3bc7a9267120d000baa1cc112ad3.jpg"
                },
                new Channel 
                {
                    StrId = "public_fengge_yaogun",
                    Id = 10,
                    Name = "摇滚",
                    CoverImage = "http://b.hiphotos.baidu.com/ting/pic/item/9345d688d43f8794e2b5f4e2d31b0ef41ad53ab7.jpg"
                },
                new Channel
                {
                    StrId = "public_fengge_minyao",
                    Id = 60,
                    Name = "民谣风景",
                    CoverImage = "http://a.hiphotos.baidu.com/ting/pic/item/d788d43f8794a4c235fe5ae70ff41bd5ac6e3918.jpg"
                },
                new Channel
                {
                    StrId = "public_fengge_qingyinyue",
                    Id = 29,
                    Name = "轻音乐",
                    CoverImage = "http://c.hiphotos.baidu.com/ting/pic/item/b2de9c82d158ccbf4c6708f618d8bc3eb0354126.jpg"
                },
                new Channel
                {
                    StrId = "public_fengge_xiaoqingxin",
                    Id = 49,
                    Name = "小清新",
                    CoverImage = "http://b.hiphotos.baidu.com/ting/pic/item/908fa0ec08fa513d81cdd0123c6d55fbb3fbd993.jpg"
                },
                new Channel 
                {
                    StrId = "public_fengge_zhongguofeng",
                    Id = 53,
                    Name = "中国风",
                    CoverImage = "http://c.hiphotos.baidu.com/ting/pic/item/6a600c338744ebf8db29d8f3d8f9d72a6159a7a6.jpg"
                },
                new Channel
                {
                    StrId = "public_fengge_dj",
                    Id = 50,
                    Name = "DJ舞曲",
                    CoverImage = "http://d.hiphotos.baidu.com/ting/pic/item/cc11728b4710b912f75c4b32c2fdfc0393452258.jpg"
                },
                new Channel 
                {
                    StrId = "public_fengge_dianyingyuansheng",
                    Id = 38,
                    Name = "电影原声",
                    CoverImage = "http://b.hiphotos.baidu.com/ting/pic/item/aec379310a55b319990a3fb942a98226cefc170e.jpg"
                },
                new Channel 
                {
                    StrId = "public_xinqing_qingsongjiari",
                    Id = 40,
                    Name = "轻松假日",
                    CoverImage = "http://a.hiphotos.baidu.com/ting/pic/item/09fa513d269759ee944bdc07b3fb43166c22dfe7.jpg"
                },
                new Channel
                {
                    StrId = "public_xinqing_tianmi",
                    Id = 17,
                    Name = "甜蜜感受",
                    CoverImage = "http://a.hiphotos.baidu.com/ting/pic/item/060828381f30e92429cc20fd4d086e061c95f796.jpg"
                },
                new Channel 
                {
                    StrId = "public_xinqing_jimo",
                    Id = 37,
                    Name = "寂寞电波",
                    CoverImage = "http://c.hiphotos.baidu.com/ting/pic/item/2934349b033b5bb5043c9d3237d3d539b700bc17.jpg"
                },
                new Channel 
                {
                    StrId = "public_xinqing_qingge",
                    Id = 56,
                    Name = "单身情歌",
                    CoverImage = "http://d.hiphotos.baidu.com/ting/pic/item/37d3d539b6003af3e92361ac342ac65c1138b665.jpg"
                },
                new Channel 
                {
                    StrId = "public_xinqing_shuhuan",
                    Id = 11,
                    Name = "舒缓节奏",
                    CoverImage = "http://c.hiphotos.baidu.com/ting/pic/item/f9dcd100baa1cd1149d9ceecb812c8fcc2ce2d8a.jpg"
                },
                new Channel 
                {
                    StrId = "public_xinqing_yonglanwuhou",
                    Id = 19,
                    Name = "慵懒午后",
                    CoverImage = "http://c.hiphotos.baidu.com/ting/pic/item/bd315c6034a85edf9ecc42ef48540923dc5475bd.jpg"
                },
                new Channel 
                {
                    StrId = "public_xinqing_shanggan",
                    Id = 36,
                    Name = "伤感调频",
                    CoverImage = "http://c.hiphotos.baidu.com/ting/pic/item/b8389b504fc2d56278df6d21e61190ef77c66c23.jpg"
                },
                new Channel 
                {
                    StrId = "public_yuzhong_huayu",
                    Id = 32,
                    Name = "华语",
                    CoverImage = "http://d.hiphotos.baidu.com/ting/pic/item/72f082025aafa40f2cea7211aa64034f79f019c9.jpg"
                },
                new Channel 
                {
                    StrId = "public_yuzhong_oumei",
                    Id = 33,
                    Name = "欧美",
                    CoverImage = "http://d.hiphotos.baidu.com/ting/pic/item/c9fcc3cec3fdfc03d0655f74d53f8794a5c226d8.jpg"
                },
                new Channel 
                {
                    StrId = "public_yuzhong_riyu",
                    Id = 34,
                    Name = "日语",
                    CoverImage = "http://d.hiphotos.baidu.com/ting/pic/item/c2fdfc039245d68890140e68a5c27d1ed31b24e3.jpg"
                },
                new Channel 
                {
                    StrId = "public_yuzhong_hanyu",
                    Id = 44,
                    Name = "韩语",
                    CoverImage = "http://c.hiphotos.baidu.com/ting/pic/item/242dd42a2834349bc934d249c8ea15ce37d3bec8.jpg"
                }
            };
        }

        private string BulidUrlForGainSongs(GainSongParameter para)
        {
            var channel = para.Channel.StrId;
            if (string.IsNullOrWhiteSpace(channel)) return string.Empty;
            int page;
            if (!_gotSongList.TryGetValue(channel, out page))
                _gotSongList.Add(channel, page);

            var url = new StringBuilder(BaseUrl).Append("&method=baidu.ting.radio.getChannelSong");
            url.Append(string.Format("&channelname={0}", channel));
            url.Append(string.Format("&n={0}", page));
            url.Append("&rn=10");
            return url.ToString();
        }

        private string BulidContentForLogin(string userName, string password, string verifyCode = "", string verifyKey = "")
        {
            var content = new StringBuilder();
            var name = Uri.EscapeDataString(userName);
            var pass = Convert.ToBase64String(Encoding.ASCII.GetBytes(password))
                              .Replace("+", "%2B").Replace("/", "%2F").Replace("=", "%3D");
            //must follow alphabetical order for get sign
            content.Append(string.Format("appid={0}&", "1"));
            content.Append(string.Format("crypttype={0}&", 1));
            content.Append(string.Format("isphone={0}&", 0));
            content.Append(string.Format("logictype={0}&", "0"));
            content.Append(string.Format("login_type={0}&", 3));
            content.Append(string.Format("password={0}&", pass));
            content.Append(string.Format("tpl={0}&", "qianqian"));
            content.Append(string.Format("username={0}&", name));
            if (!string.IsNullOrWhiteSpace(verifyCode) && !string.IsNullOrWhiteSpace(verifyKey))
            {
                content.Append(string.Format("vcodestr={0}&", verifyKey));
                content.Append(string.Format("verifycode={0}&", Uri.EscapeDataString(verifyCode)));
            }

            var sign = GetContentSign(content.ToString());
            content.Append(string.Format("sig={0}", sign));
            return content.ToString();
        }

        private string GetContentSign(string content)
        {
            var str = string.Format("{0}sign_key=3a15f3fef06e24b4109f4c060dbca587", content);
            return AESHelper.Md5Hash(str);
        }

        private void GetSongInfo(Song song)
        {
            var url = new StringBuilder(BaseUrl).Append("&method=baidu.ting.song.getInfos");
            var para = string.Format("&songid={0}&ts={1}", song.Sid, Environment.TickCount);
            url.Append(para);
            url.Append(string.Format("&e={0}", AESForTing.Encrypt(para)));

            try
            {
                var json = HttpWebDealer.GetJsonObject(url.ToString(), _headers, Encoding.UTF8);
                if (json["error_code"].ToString() != BaiduJsonErrorCode.OK)
                {
                    LoggerHelper.Instance.Msg("Baidu Json Error Code: " + json["error_code"], url);
                    return;
                }
                var info = json["songinfo"];
                var urls = (IEnumerable)json["songurl"]["url"];
                song.AlbumTitle = info["album_title"];
                song.AlbumId = info["album_id"];
                song.LrcUrl = info["lrclink"];
                song.Thumb = info["pic_small"];
                song.Picture = info["pic_premium"];

                var rate = 0;
                foreach (dynamic uri in urls)
                {
                    var r = uri["file_bitrate"];
                    if (r <= rate) continue;
                    song.Kbps = rate = r;
                    song.Url = uri["file_link"];
                }
            }
            catch (Exception e)
            {
                LoggerHelper.Instance.Exception(e);
            }
        }

        private bool SongFavoriteOperation(GainSongParameter para)
        {
            switch (para.OperationType)
            {
                case OperationType.Hate:
                case OperationType.Played:
                    break;
                case OperationType.Like:
                case OperationType.DisLike:
                    try
                    {
                        var url = BulidUrlForSongFavorite(para);
                        var json = HttpWebDealer.GetJsonObject(url, _headers, Encoding.UTF8);
                        return json["error_code"].ToString() == BaiduJsonErrorCode.OK;
                    }
                    catch (Exception e)
                    {
                        Logger.Exception(e);
                        return false;
                    }
            }
            return true;
        }

        private string BulidUrlForSongFavorite(GainSongParameter para)
        {
            if (para.Account == null || string.IsNullOrWhiteSpace(para.Account.BdUss) ||
                string.IsNullOrWhiteSpace(para.SongId)) return string.Empty;

            var method = string.Empty;
            switch (para.OperationType)
            {
                case OperationType.Like:
                    method = "baidu.ting.favorite.addSongFavorite";
                    break;
                case OperationType.DisLike:
                    method = "baidu.ting.favorite.delSongFavorite";
                    break;
                case OperationType.Hate:
                case OperationType.Played:
                    //Now do nothing with these two kinds of operation
                    break;
            }
            if (string.IsNullOrWhiteSpace(method)) return string.Empty;
            var url = new StringBuilder(BaseUrl);
            url.Append(string.Format("&method={0}", method));
            url.Append(string.Format("&bduss={0}", para.Account.BdUss));
            url.Append(string.Format("&songId={0}", para.SongId));

            return url.ToString();
        }
        
    }
}
