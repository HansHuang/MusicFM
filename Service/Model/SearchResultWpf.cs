using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonHelperLibrary;

namespace Service.Model
{
    public class SearchResultWpf : SearchResult
    {
        public new ObservableCollection<Song> SongList { get; set; }
        public new ObservableCollection<Channel> ChannelList { get; set; }

        public SearchResultWpf()
        {

        }

        public SearchResultWpf(SearchResult result)
        {
            Query = result.Query;
            ResultCount = result.ResultCount;
            CurrentNr = result.CurrentNr;
            if (result.Artist != null)
                Artist = result.Artist.Serialize().Deserialize<Artist>();

            if (result.SongList != null && result.SongList.Count > 0)
                SongList = new ObservableCollection<Song>(result.SongList);

            if (result.ChannelList != null && result.ChannelList.Count > 1)
                ChannelList = new ObservableCollection<Channel>(result.ChannelList);
        }
    }
}
