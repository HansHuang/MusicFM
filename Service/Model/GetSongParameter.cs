using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Model
{
    public class SongActionParameter 
    {
        public Account Account { get; set; }

        public string SongId { get; set; }
        public Channel Channel { get; set; }
        //Song played position(second, one decimal place)
        public string Position { get; set; }

        public SongActionParameter (){}

        public SongActionParameter(Account account) 
        {
            Account = account;
        }

        /// <summary>
        /// Get the postion second of song played(one decimal place)
        /// </summary>
        /// <param name="seconds">second, one decimal place</param>
        /// <returns></returns>
        public SongActionParameter PositionSeconds(decimal seconds)
        {
            Position = seconds.ToString("N1");
            return this;
        }

        /// <summary>
        /// Get the postion second of song played
        /// </summary>
        /// <param name="seconds">second</param>
        /// <returns></returns>
        public SongActionParameter PositionSeconds(int seconds)
        {
            Position = seconds.ToString("N1");
            return this;
        }

        /// <summary>
        /// Get Current song id
        /// </summary>
        /// <param name="song"></param>
        /// <returns></returns>
        public SongActionParameter CurrentSongID(Song song) 
        {
            if (song != null)
                SongId = song.Sid.ToString();
            return this;
        }

        /// <summary>
        /// GEt current channel id
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public SongActionParameter CurrentChennal(Channel channel) 
        {
            Channel = channel ?? new Channel();
            return this;
        }
    }

    public class GainSongParameter:SongActionParameter
    {
        
        public string History { get; set; }
        public OperationType OperationType { get; set; }

        public GainSongParameter(){}

        public GainSongParameter(Account account) : base(account) {}

        /// <summary>
        /// Get history string inside
        /// </summary>
        /// <param name="songs">Song list(20 elements at most)</param>
        /// <param name="type">Operation type</param>
        public GainSongParameter HistoryString(ICollection<Song> songs, OperationType type) 
        {
            OperationType = type;
            History = string.Empty;
            if (songs == null) return this;
            songs = songs.Where(s => s != null).ToList();
            if (songs.Count < 1) return this;
            var sb = new StringBuilder();
            if (songs.Count > 20) songs = songs.Skip(songs.Count - 20).ToList();

            foreach (var song in songs.Where(s => s != null))
            {
                sb.Append(song.Sid + ":p|");
            }
            var lastAction = ":p|";
            switch (type)
            {
                case OperationType.Like:
                    lastAction = ":r|";
                    break;
                case OperationType.DisLike:
                    lastAction = ":u|";
                    break;
                case OperationType.Hate:
                    lastAction = ":s|";
                    break;
                case OperationType.Played:
                    lastAction = ":p|";
                    break;
            }
            var history = sb.ToString();
            History = history.Substring(0, history.Length - 3) + lastAction;
            return this;
        }
    }
}
