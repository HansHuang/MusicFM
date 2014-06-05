using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonHelperLibrary;

namespace Service.Model
{
    [Serializable]
    public class SearchResult
    {
        public string Query { get; set; }
        public int ResultCount { get; set; }
        public int CurrentNr { get; set; }
        public Artist Artist { get; set; }
        public List<Song> SongList { get; set; }
        public List<Channel> ChannelList { get; set; }

        public SearchResult()
        {
            SongList = new List<Song>();
            ChannelList = new List<Channel>();
        }

        public static SearchResult Copy(SearchResult source) 
        {
            source = source ?? new SearchResult();
            return source.Serialize().Deserialize<SearchResult>();
        }
    }
}
