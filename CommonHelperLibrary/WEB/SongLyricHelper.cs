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
    /// Class : DoubanFm
    /// Discription : Common helper for get song lyric
    /// </summary>
    public static class SongLyricHelper
    {
        /// <summary>
        /// Get song's lyric(String Format)
        /// </summary>
        /// <param name="title">song title</param>
        /// <param name="artist">song artist</param>
        /// <returns></returns>
        public static string GetSongLrc(string title, string artist)
        {
            var artist2 = string.Empty;
            if (artist.Contains("/")) 
            {
                var artists = artist.Split(new[] {"/"}, StringSplitOptions.RemoveEmptyEntries);
                artist = artists[0];
                if (artists.Length > 1) artist2 = artists[1];
            }
            if (title.Contains("("))
                title = title.Split(new[] {'(', ')'}, StringSplitOptions.RemoveEmptyEntries)[0];
            var uTitle = System.Web.HttpUtility.UrlEncode(title.Trim());
            var uArtist = System.Web.HttpUtility.UrlEncode(artist);
            //Get lrc search result
            var response = HttpWebDealer.GetHtml(
              string.Format("http://box.zhangmen.baidu.com/x?op=12&count=1&title={0}$${1}$$$$", uTitle, uArtist));
            if (string.IsNullOrEmpty(response)) return string.Empty;

            //Console.WriteLine(response);
            //Get lrc id
            var xml = new XmlDocument();
            xml.LoadXml(response);
            var list = xml.GetElementsByTagName("lrcid");
            if (list.Count == 0 || list[0].InnerText.Equals("0"))
                return string.IsNullOrEmpty(artist) ? string.Empty : GetSongLrc(title, artist2);

            int lId;
            if (list.Count < 1 || !int.TryParse(list[0].InnerText, out lId) || lId < 1) return string.Empty;
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
            var str = GetSongLrc(title, artist);
            if (string.IsNullOrEmpty(str)) return null;
            str = str.Replace(" [", "\r\n");
            var lines = str.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 1) return null;

            var lrc = new SongLyric();
            var reg0 = new Regex(@"\[([a-z]+)\:(.+)]");
            var reg1 = new Regex(@"([\[\d{2}\:\d{2}\.\d{1,3}\]]+)(.*)");
            var filters = new List<string>() { "☆", "51lrc", "@", "LRC by", "Lyrics by", "歌词吾爱",
                "歌词库", "Lyriced By", "编辑：", "QQ", "PS：", "★", "http","www.","by:","制作",".com"};
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line.Trim())) continue;
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
                        var ts = time.Split(new[] {':', '.'}, StringSplitOptions.RemoveEmptyEntries);
                        TimeSpan key;
                        if (ts.Length == 2)
                            key = new TimeSpan(0, 0, Convert.ToInt32(ts[0]), Convert.ToInt32(ts[1]));
                        else if (ts.Length == 3)
                            key = new TimeSpan(0, 0, Convert.ToInt32(ts[0]), Convert.ToInt32(ts[1]), Convert.ToInt32(ts[2]));
                        else
                            key = new TimeSpan();
                        if(!lrc.Content.ContainsKey(key))
                            lrc.Content.Add(key, txt);
                    }
                }
            }
            var odered = lrc.Content.OrderBy(s => s.Key).ToList();
            lrc.Content.Clear();
            odered.ForEach(s => lrc.Content.Add(s.Key, s.Value));
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

        public SongLyric()
        {
            Content = new Dictionary<TimeSpan, string>();
        }
    }
}
