using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace CommonHelperLibrary.WEB
{
    /// <summary>
    /// Author : Hans Huang
    /// Date : 2013-10-25
    /// Class : SongLyricHelper
    /// Discription : Common helper for get song lyric
    /// </summary>
    public static class SongLyricHelper
    {
        /// <summary>
        /// Get song's lyric(String Format)
        /// </summary>
        /// <param name="title">song title</param>
        /// <param name="artist">song artist</param> 
        /// <param name="mp3Urls">out song mp3 file url list</param> 
        /// <returns></returns>
        public static string GetSongLrc(string title, string artist, out List<string> mp3Urls)
        {
            mp3Urls = new List<string>();
            var artist2 = string.Empty;
            if (artist.Contains("/")) 
            {
                var artists = artist.Split(new[] {"/"}, StringSplitOptions.RemoveEmptyEntries);
                artist = artists[0];
                if (artists.Length > 1) artist2 = artists[1];
            }
            //if (title.Contains("(") || title.Contains("（"))
                title = title.Split(new[] { '(', ')', '（','）' }, StringSplitOptions.RemoveEmptyEntries)[0];
            var uTitle = System.Web.HttpUtility.UrlEncode(title.Trim());
            var uArtist = System.Web.HttpUtility.UrlEncode(artist);
            //Get lrc search result
            var response = HttpWebDealer.GetHtml(
              string.Format("http://box.zhangmen.baidu.com/x?op=12&count=1&title={0}$${1}$$$$", uTitle, uArtist));
            if (string.IsNullOrEmpty(response)) return string.Empty;

            //Get lrc id
            var xml = new XmlDocument();
            xml.LoadXml(response);
            var lrcList = xml.GetElementsByTagName("lrcid");
            if (lrcList.Count == 0 || lrcList[0].InnerText.Equals("0"))
                return string.IsNullOrEmpty(artist) ? string.Empty : GetSongLrc(title, artist2, out mp3Urls);

            //Get mp3 file url list
            var part1 = xml.GetElementsByTagName("encode");
            var part2 = xml.GetElementsByTagName("decode");
            if (part1.Count.Equals(part2.Count))
            {
                for (var i = 0; i < part1.Count; i++)
                {
                    var part1Txt = part1[i].InnerText.Trim();
                    var part2Txt = part2[i].InnerText.Trim();
                    var encode = part1Txt.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries).Last();
                    var mp3Url = part1Txt.Replace(encode, part2Txt);
                    if (!mp3Urls.Contains(mp3Url)) mp3Urls.Add(mp3Url);
                }
            }

            int lId;
            if (lrcList.Count < 1 || !int.TryParse(lrcList[0].InnerText, out lId) || lId < 1) return string.Empty;
            var lrc = HttpWebDealer.GetHtml(string.Format("http://box.zhangmen.baidu.com/bdlrc/{0}/{1}.lrc", Math.Floor((decimal)lId / 100), lId));
            return lrc;
        }

        /// <summary>
        /// Get song's lyric(SongLyric Format)
        /// </summary>
        /// <param name="title">song title</param>
        /// <param name="artist">song artist</param>
        /// <returns></returns>
        public static SongLyric GetSongLyric(string title, string artist)
        {
            List<string> mp3Urls;
            var str = GetSongLrc(title, artist, out mp3Urls);
            var lrc = BulidSongLyric(str);
            if (lrc != null) lrc.Mp3Urls = mp3Urls;
            return lrc;
        }

        /// <summary>
        /// Bulid SongLyric Class from lrc string
        /// </summary>
        /// <param name="lrcStr"></param>
        /// <returns></returns>
        public static SongLyric BulidSongLyric(string lrcStr)
        {
            if (string.IsNullOrEmpty(lrcStr)) return null;
            lrcStr = lrcStr.Replace(" [", "\r\n");
            var lines = lrcStr.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 1) return null;

            var lrc = new SongLyric();
            var reg0 = new Regex(@"\[([a-z]+)\:(.+)]");
            var reg1 = new Regex(@"([\[\d{2}\:\d{2}\.\d{1,3}\]]+)(.*)");
            var filters = new List<string> { "☆", "51lrc", "@", "LRC by", "Lyrics by", "歌词", "Lyriced By", "编辑：", "QQ", "PS：", "★", "http", "www.", "by:", "制作", ".com", "lrc:" };
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line.Trim())) continue;
                var usefull = true;
                filters.ForEach(s => { usefull = usefull && !line.Contains(s); });
                if (!usefull) continue;

                var m0 = reg0.Match(line);
                var m1 = reg1.Match(line);
                if (m0.Groups.Count > 1)
                {
                    if (string.IsNullOrEmpty(m0.Groups[2].Value.Trim())) continue;
                    if (m0.Groups[1].Value.Equals("ti")) lrc.Title = m0.Groups[2].Value;
                    if (m0.Groups[1].Value.Equals("ar")) lrc.Artist = m0.Groups[2].Value;
                    if (m0.Groups[1].Value.Equals("al")) lrc.Album = m0.Groups[2].Value;
                    if (m0.Groups[1].Value.Equals("offset")) lrc.Offset = Convert.ToInt32(m0.Groups[2].Value);
                    if (m0.Groups[1].Value.Equals("by")) filters.Add(m0.Groups[2].Value);
                }
                else
                {
                    if (string.IsNullOrEmpty(m1.Groups[2].Value.Trim())) continue;
                    var list = m1.Groups[1].Value.Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                    var txt = m1.Groups[2].Value.Trim();
                    foreach (var time in list)
                    {
                        var ts = time.Split(new[] { ':', '.' }, StringSplitOptions.RemoveEmptyEntries);
                        TimeSpan key;
                        if (ts.Length == 2)
                            key = new TimeSpan(0, 0, Convert.ToInt32(ts[0]), Convert.ToInt32(ts[1]));
                        else if (ts.Length == 3)
                            key = new TimeSpan(0, 0, Convert.ToInt32(ts[0]), Convert.ToInt32(ts[1]), Convert.ToInt32(ts[2]));
                        else
                            key = new TimeSpan();
                        if (!lrc.Content.ContainsKey(key))
                            lrc.Content.Add(key, txt);
                    }
                }
            }
            var odered = lrc.Content.OrderBy(s => s.Key).ToList();
            lrc.Content.Clear();
            odered.ForEach(s => lrc.Content.Add(s.Key, s.Value));
            //mp3Urls.ForEach(Console.WriteLine);
            return lrc;
        }
    }

    public class SongLyric
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public int Offset { get; set; }
        public Dictionary<TimeSpan, string> Content { get; set; }
        public List<string> Mp3Urls { get; set; }

        public SongLyric()
        {
            Content = new Dictionary<TimeSpan, string>();
            Mp3Urls = new List<string>();
        }
    }
}
