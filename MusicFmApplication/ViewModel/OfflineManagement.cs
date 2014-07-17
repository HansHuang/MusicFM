using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Input;
using CommonHelperLibrary;
using CommonHelperLibrary.WEB;
using CustomControlResources;
using MusicFmApplication.Model;
using ServiceModel = Service.Model;

namespace MusicFmApplication.ViewModel
{
    public class OfflineManagement : ViewModelBase
    {

        #region Fields
        protected readonly MainViewModel ViewModel;
        public readonly string OfflineFolder;
        protected Dictionary<Channel, List<Song>> SongListInChannel = new Dictionary<Channel, List<Song>>();
        #endregion

        #region IsInternetConnected (INotifyPropertyChanged Property)

        private bool _isInternetConnected;

        public bool IsInternetConnected
        {
            get { return _isInternetConnected; }
            set
            {
                if (_isInternetConnected.Equals(value)) return;
                _isInternetConnected = value;
                RaisePropertyChanged("IsInternetConnected");
            }
        }

        #endregion

        #region RelayCommand SetChannelOfflineCmd

        private RelayCommand _setChannelOfflineCmd;

        public ICommand SetChannelOfflineCmd
        {
            get { return _setChannelOfflineCmd ?? (_setChannelOfflineCmd = new RelayCommand(c => SetchannelOfflineExecute(c as Channel))); }
        }

        private async void SetchannelOfflineExecute(Channel channel)
        {
            //Not allow current playing channel
            if (channel == null || (ViewModel.CurrentChannel == channel && !IsInternetConnected)) return;
            ViewModel.MediaManager.IsBuffering = true;

            var t = Task.Run(() =>
            {
                var folder = OfflineFolder + string.Format("{0}({1})\\", channel.StrId, channel.Id);
                //Clear offline data
                if (channel.IsOfflined)
                {
                    if (Directory.Exists(folder)) Directory.Delete(folder, true);
                    SongListInChannel.Remove(channel);
                    return false;
                }
                return DownloadSongs(folder, channel);
            });

            try
            {
                channel.IsOfflined = await t;
            }
            catch (Exception e)
            {
                App.Log.Exception(e);
            }
            ViewModel.MediaManager.IsBuffering = false;
        }

        private bool DownloadSongs(string folder, Channel channel)
        {
            //1. Create Folders
            var picFolder = folder + "Picture\\";
            var songFolder = folder + "Song\\";
            var lrcFolder = folder + "Lyric\\";
            if (!DirectoryHelper.MakeSureExist(folder) || !DirectoryHelper.MakeSureExist(picFolder) ||
                !DirectoryHelper.MakeSureExist(songFolder) || !DirectoryHelper.MakeSureExist(lrcFolder))
                return false;
            //2. Get song data
            var songList = new List<Song>();
            while (songList.Count < ViewModel.Setting.ChannelOfflineSize.GetValueOrDefault())
            {
                songList.AddRange(ViewModel.GetSongListByChannel(channel));
            }
            //3. Download
            DownloadProgressChangedEventHandler downloadMonitor = delegate(object s, DownloadProgressChangedEventArgs e)
            {
                if (!channel.IsOfflined) return;
                channel.DownloadProgress = e.ProgressPercentage;
            };
            foreach (var song in songList)
            {
                if (string.IsNullOrWhiteSpace(song.LrcUrl))
                    song.LrcUrl = SongLyricHelper.GetSongLrcPath(song.Title, song.Artist);

                //bulid file name
                var nameBase = song.Artist + "-" + song.Title;
                var songName = nameBase + ".mp3";
                var picName = nameBase + ".jpg";
                var thumbName = nameBase + ".thumb.jpg";
                var lrcName = nameBase + ".lrc";
                //download file to local
                HttpWebDealer.DownloadFile(songName, song.Url, songFolder, downloadMonitor);
                HttpWebDealer.DownloadFile(picName, song.Picture, picFolder);
                HttpWebDealer.DownloadFile(thumbName, song.Thumb, picFolder);
                HttpWebDealer.DownloadFile(lrcName, song.LrcUrl, lrcFolder);
                //change each path in song
                song.Url = songFolder + songName;
                song.Picture = picFolder + picName;
                song.Thumb = picFolder + thumbName;
                song.LrcUrl = lrcFolder + lrcName;
            }
            //Save song data to file
            using (var sr = new StreamWriter(folder + "Song.dat", false))
            {
                sr.Write(songList.SerializeToJson());
            }
            //Save song data to file
            using (var sr = new StreamWriter(folder + "Channel.dat", false))
            {
                sr.Write(channel.SerializeToString());
            }
            return true;
        }

        #endregion

        public OfflineManagement(MainViewModel viewModel)
        {
            ViewModel = viewModel;

            OfflineFolder = Environment.CurrentDirectory + "\\OfflineData\\";
            if (!Directory.Exists(OfflineFolder)) Directory.CreateDirectory(OfflineFolder);

            IsInternetConnected = InternetHelper.IsConnected;
            //IsInternetConnected = false;
            ViewModel.DownloadProgress = 100;
        }

        public void StartOfflinePlayer()
        {
            GetOfflineChannels();

            ViewModel.Channels = new ObservableCollection<Channel>(SongListInChannel.Keys);
            ViewModel.CurrentChannel = SongListInChannel.Keys.FirstOrDefault();

            if (ViewModel.CurrentChannel != null)
                ViewModel.NextSongCmd.Execute(false);
        }

        public void NextSongExecute(bool? isEnded = false)
        {
            if (isEnded.GetValueOrDefault())
                ViewModel.HistorySongList.Insert(0, ViewModel.CurrentSong);
            if (ViewModel.SongList.Count < 1)
                ViewModel.SongList = new ObservableCollection<Song>(SongListInChannel[ViewModel.CurrentChannel]);
            ViewModel.CurrentSong = ViewModel.SongList[0];
            ViewModel.MediaManager.StartPlayerCmd.Execute(null);
            ViewModel.SongList.RemoveAt(0);
        }

        public void LikeSongExecute(string isHate)
        {
            var sId = ViewModel.CurrentSong.Sid;
            int like;
            if (string.IsNullOrWhiteSpace(isHate))
                ViewModel.CurrentSong.Like = like = ViewModel.CurrentSong.Like == 0 ? 1 : 0;
            else
            {
                ViewModel.CurrentSong.Like = like = -1;
                ViewModel.NextSongCmd.Execute(false);
            }

            //save change to json file
            Task.Run(() =>
            {
                var channel = ViewModel.CurrentChannel;
                var songJson = OfflineFolder + string.Format("{0}({1})\\", channel.StrId, channel.Id) + "\\Song.dat";
                if (!File.Exists(songJson)) return;
                var songList = File.ReadAllText(songJson).DeserializeFromJson<List<Song>>();
                if (songList == null) return;
                var song = songList.FirstOrDefault(s => s.Sid == sId);
                if (song == null) return;
                song.Like = like;
                using (var sr = new StreamWriter(songJson, false))
                {
                    sr.Write(songList.SerializeToJson());
                }
            });
        }

        public void GetOfflineLyric()
        {
            Task.Run(() =>
            {
                if (ViewModel.CurrentSong == null) return null;
                var lrc = SongLyricHelper.GetSongLyric(ViewModel.CurrentSong.LrcUrl);
                return lrc;
            }).ContinueWith(t =>
            {
                ViewModel.MediaManager.Lyric = t.Result;
            }, new CancellationToken(), TaskContinuationOptions.None, ViewModel.ContextTaskScheduler);
        }

        public void OpenDownloadFolderExecute()
        {
            Task.Run(() =>
            {
                Process.Start("Explorer.exe", "/select," + ViewModel.CurrentSong.Url);
            });
        }

        private void GetOfflineChannels()
        {
            var dirList = Directory.GetDirectories(OfflineFolder);

            foreach (var dir in dirList)
            {
                var channelData = dir + "\\Channel.dat";
                var songData = dir + "\\Song.dat";
                if (!File.Exists(channelData) || !File.Exists(songData)) continue;
                try
                {
                    var channel = File.ReadAllText(channelData).Deserialize<Channel>();
                    var songList = File.ReadAllText(songData).DeserializeFromJson<List<Song>>();
                    if (channel == null || songList == null || songList.Count < 1) continue;
                    channel.IsOfflined = true;
                    channel.DownloadProgress = 100;
                    SongListInChannel.Add(channel, songList);
                }
                catch { continue; }
            }
        }


    }
}