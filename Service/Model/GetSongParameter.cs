﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Model
{
    public class GainSongParameter
    {
        public string UserId { get; set; }
        public string Expire { get; set; }
        public string Token { get; set; }
        public string History { get; set; }
        public string SongId { get; set; }
        public int ChannelId { get; set; }
        //Get from webpage
        public string Cookie { get; set; }
        //Song played position(second, one decimal place)
        public decimal Position { get; set; }

        public GainSongParameter(){}

        /// <summary>
        /// Bulid history string inside
        /// </summary>
        /// <param name="songs">Song list(20 elements at most)</param>
        /// <param name="type">Operation type</param>
        public GainSongParameter(ICollection<Song> songs, OperationType type)
        {
            if (songs == null || songs.Count < 1) return;
            var sb = new StringBuilder();
            if (songs.Count > 20) songs = songs.Skip(songs.Count - 20).ToList();

            foreach (var song in songs)
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
            }
            var history = sb.ToString();
            History = history.Substring(0, history.Length - 3) + lastAction; ;
        }
    }
}
