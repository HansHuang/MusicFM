﻿using System;
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
        List<Song> GetSongList(GainSongParameter parameter);
        List<Channel> GetChannels(bool isBasic = true);
        bool CompletedSong(SongActionParameter parameter);
        Account Login(string userName, string password, AccountType type);
        SongLyric GetLyric(Song song);
    }
}
