using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonHelperLibrary;
using CommonHelperLibrary.WEB;
using MvPlayer.Service.Model;

namespace MvPlayer.Service
{
    public class YinYueTai
    {
        public static List<MusicVideo> GetIndexMvList(IndexMvType type, IndexMvArea area)
        {
            var mvList = new List<MusicVideo>();
            var typeStr = EnumHelper.GetEnumDescription(type);
            var areaStr = EnumHelper.GetEnumDescription(area);

            var json = HttpWebDealer.GetJsonObject(string.Format("http://www.yinyuetai.com/ajax/{0}?area={1}", typeStr, areaStr),null,Encoding.UTF8);
            foreach (var item in json) 
            {
                var mv = new MusicVideo
                {
                    Id = item["videoId"],
                    Title = item["title"],
                    Image = item["image"],
                    PlayPageUrl = "http://v.yinyuetai.com/video/" + item["videoId"]
                };
                foreach (var arts in item["artists"])
                {
                    mv.Artists.Add(new MvArtist
                    {
                        Id = arts["id"],
                        Name = arts["artistName"],
                        MainPage = "http://www.yinyuetai.com/fanclub/mv/" + arts["id"] + "/toNew"
                    });
                }
                mvList.Add(mv);
            }
            return mvList;
        }

        public enum IndexMvType 
        {
            [Description("shoubo")]
            Premiere,
            [Description("zhengliuxing")]
            Trending
        }

        public enum IndexMvArea
        {
            [Description("all")]
            All,
            [Description("ml")]
            Mainland,
            [Description("ht")]
            HongKongTaiwan,
            [Description("us")]
            EuropeAmerica,
            [Description("kr")]
            Korea,
            [Description("jp")]
            Japan
        }
    }
}
