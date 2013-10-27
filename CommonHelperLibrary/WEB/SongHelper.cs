using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace CommonHelperLibrary.WEB
{
    //http://bbs.aau.cn/forum.php?mod=viewthread&tid=10977
    /// <summary>
    /// Web application helper
    /// </summary>
    public static class SongHelper
    {
        /// <summary>
        /// Get song's lyric(String Format)
        /// </summary>
        /// <param name="title">song title</param>
        /// <param name="artist">song artist</param>
        /// <returns></returns>
        public static string GetSongLrc(string title, string artist)
        {
            title = HttpUtility.UrlEncode(title.Trim());
            artist = HttpUtility.UrlEncode(artist.Trim());
            //Get lrc search result
            var response = HttpWebDealer.GetHtml(
              string.Format("http://box.zhangmen.baidu.com/x?op=12&count=1&title={0}$${1}$$$$", title, artist));
            if (string.IsNullOrEmpty(response)) return string.Empty;

            //Get lrc id
            var xml = new XmlDocument();
            xml.LoadXml(response);
            var list = xml.GetElementsByTagName("lrcid");
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
            var lines = str.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 1) return null;

            var lrc = new SongLyric();
            var reg0 = new Regex(@"\[([a-z]+)\:(.+)]");
            var reg1 = new Regex(@"([\[\d{2}\:\d{2}\.\d{1,3}\]]+)(.*)");
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line.Trim())) continue;
                var m0 = reg0.Match(line);
                var m1 = reg1.Match(line);
                if (m0.Groups.Count > 1)
                {
                    if (m0.Groups[1].Value.Equals("ti")) lrc.Title = m0.Groups[2].Value;
                    if (m0.Groups[1].Value.Equals("ar")) lrc.Artist = m0.Groups[2].Value;
                    if (m0.Groups[1].Value.Equals("al")) lrc.Album = m0.Groups[2].Value;
                    if (m0.Groups[1].Value.Equals("offset")) lrc.Offset = Convert.ToInt32(m0.Groups[2].Value);
                }
                else
                {
                    var list = m1.Groups[1].Value.Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var s in list)
                        lrc.Content.Add(s, m1.Groups[2].Value.Trim());
                }
            }
            return lrc;
        }


        public class SongLyric
        {
            public string Title { get; set; }
            public string Artist { get; set; }
            public string Album { get; set; }
            public int Offset { get; set; }
            public Dictionary<string, string> Content { get; set; }

            public SongLyric()
            {
                Content = new Dictionary<string, string>();
            }
        }
    }
}
