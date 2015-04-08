using System.Collections.Generic;
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

        Task<List<Song>> GetSongList(GainSongParameter parameter);
        Task<List<Channel>> GetChannels(bool isBasic = true);
        Task<bool> CompletedSong(SongActionParameter parameter);
        Task<Account> Login(string userName, string password, AccountType type);
        SongLyric GetLyric(Song song);
        Task<SearchResult> Search(string keyword, int count);
        Task<List<Song>> GetSongList(Artist artist);
    }
}
