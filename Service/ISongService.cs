using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonHelperLibrary.WEB;
using Service.Model;

namespace Service
{
    public interface ISongService 
    {
        string Name { get; }
        string LocalizeName { get; set; }
        List<AccountType> AvaliableAccountTypes { get; }

        List<Song> GetSongList(GainSongParameter parameter);
        List<Channel> GetChannels(bool isBasic = true);
        bool CompletedSong(SongActionParameter parameter);
        Account Login(string userName, string password, AccountType type);
        SongLyric GetLyric(Song song);
        SearchResult Search(string keyword, int count);
        List<Song> GetSongList(Artist artist);
    }
}
