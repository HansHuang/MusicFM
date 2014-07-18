using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonHelperLibrary;
using ServiceModel=Service.Model;

namespace MusicFm.Model
{
    public class SearchResult : ServiceModel.SearchResult
    {
        public new ObservableCollection<Song> SongList { get; set; }
        public new ObservableCollection<Channel> ChannelList { get; set; }

        public SearchResult() { }

        public SearchResult(ServiceModel.SearchResult result)
            : base(result)
        {
            if (result.SongList != null && result.SongList.Count > 0)
                SongList = new ObservableCollection<Song>(result.SongList.Select(s => new Song(s)));

            if (result.ChannelList != null && result.ChannelList.Count > 1)
                ChannelList = new ObservableCollection<Channel>(result.ChannelList.Select(s => new Channel(s)));
        }
    }
}
