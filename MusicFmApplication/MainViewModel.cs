﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonHelperLibrary;
using CommonHelperLibrary.WEB;
using CustomControlResources;
using Service;
using Service.Model;

namespace MusicFmApplication
{
    /// <summary>
    /// Author : Hans Huang
    /// Date : 2013-10-19
    /// Class : MainViewModel
    /// Discription : ViewModel of MainWindow
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged RaisePropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Fields

        private static MainViewModel _instance;

        public MainWindow MainWindow { get; private set; }

        private const string SongListCacheName = "SongList";
        private const string SongListExpireCacheName = "SongListExpire";

        [ImportMany(typeof(ISongService))]
        public List<ISongService> AvalibleSongServices { get; set; }

        #endregion

        #region Notify Properties

        #region MediaManager (INotifyPropertyChanged Property)

        private MediaManager _mediaManager;

        public MediaManager MediaManager
        {
            get { return _mediaManager; }
            private set
            {
                if (_mediaManager != null && _mediaManager.Equals(value)) return;
                _mediaManager = value;
                RaisePropertyChanged("MediaManager");
            }
        }
        #endregion

        #region Account (INotifyPropertyChanged Property)

        private AccountManager _account;

        public AccountManager Account
        {
            get { return _account; }
            private set
            {
                if (_account != null && _account.Equals(value)) return;
                _account = value;
                RaisePropertyChanged("Account");
            }
        }

        #endregion

        #region WeatherMgr (INotifyPropertyChanged Property)

        private WeatherManager _weatherMgr;

        public WeatherManager WeatherMgr
        {
            get { return _weatherMgr; }
            private set
            {
                if (_weatherMgr != null && _weatherMgr.Equals(value)) return;
                _weatherMgr = value;
                RaisePropertyChanged("WeatherMgr");
            }
        }

        #endregion
        
        #region IsShowPlayerDetail (INotifyPropertyChanged Property)

        private bool _isShowPlayerDetail;
        public bool IsShowPlayerDetail
        {
            get { return _isShowPlayerDetail; }
            set
            {
                if (_isShowPlayerDetail.Equals(value)) return;
                _isShowPlayerDetail = value;
                RaisePropertyChanged("IsShowPlayerDetail");
            }
        }
        #endregion

        #region SongService (INotifyPropertyChanged Property)

        private ISongService _songService;

        public ISongService SongService
        {
            get { return _songService; }
            set
            {
                if (_songService != null && _songService.Equals(value)) return;
                _songService = value;
                RaisePropertyChanged("SongService");
            }
        }

        #endregion

        #region HistorySongList (INotifyPropertyChanged Property)

        private ObservableCollection<Song> _historySongList;

        public ObservableCollection<Song> HistorySongList
        {
            get { return _historySongList ?? (_historySongList = new ObservableCollection<Song>()); }
            set
            {
                if (_historySongList != null && _historySongList.Equals(value)) return;
                _historySongList = value;
                RaisePropertyChanged("HistorySongList");
            }
        }
        #endregion

        #region SongList (INotifyPropertyChanged Property)

        private ObservableCollection<Song> _songList;

        public ObservableCollection<Song> SongList
        {
            get { return _songList ?? (_songList = new ObservableCollection<Song>()); }
            set
            {
                if (_songList != null && _songList.Equals(value)) return;
                _songList = value;
                RaisePropertyChanged("SongList");
            }
        }

        #endregion

        #region CurrentSong (INotifyPropertyChanged Property)

        private Song _currentSong;

        public Song CurrentSong
        {
            get { return _currentSong; }
            set
            {
                if (_currentSong != null && _currentSong.Equals(value)) return;
                _currentSong = value;
                RaisePropertyChanged("CurrentSong");
            }
        }
        #endregion

        #region IsDisylayLyric (INotifyPropertyChanged Property)

        private bool _isDisylayLyric;
        public bool IsDisylayLyric
        {
            get { return _isDisylayLyric; }
            set
            {
                if (_isDisylayLyric.Equals(value)) return;
                _isDisylayLyric = value;
                RaisePropertyChanged("IsDisylayLyric");
            }
        }
        #endregion

        #region Channels (INotifyPropertyChanged Property)

        private ObservableCollection<Channel> _channels;

        public ObservableCollection<Channel> Channels
        {
            get { return _channels??(_channels=new ObservableCollection<Channel>()); }
            set
            {
                if (_channels != null && _channels.Equals(value)) return;
                _channels = value;
                RaisePropertyChanged("Channels");
            }
        }
        #endregion

        #region CurrentChannel (INotifyPropertyChanged Property)

        private Channel _currentChannel;

        public Channel CurrentChannel
        {
            get { return _currentChannel; }
            set
            {
                if (_currentChannel != null && _currentChannel.Equals(value)) return;
                _currentChannel = value;
                RaisePropertyChanged("CurrentChannel");
            }
        }
        #endregion

        #region IsDownlading (INotifyPropertyChanged Property)

        private bool _isDownlading;

        public bool IsDownlading
        {
            get { return _isDownlading; }
            set
            {
                if (_isDownlading.Equals(value)) return;
                _isDownlading = value;
                RaisePropertyChanged("IsDownlading");
            }
        }

        #endregion

        #region DownloadProgress (INotifyPropertyChanged Property)

        private int _downloadProgress;

        public int DownloadProgress
        {
            get { return _downloadProgress; }
            set
            {
                if (_downloadProgress.Equals(value)) return;
                _downloadProgress = value;
                RaisePropertyChanged("DownloadProgress");
            }
        }

        #endregion

        #endregion

        #region Delegate Commands

        #region RelayCommand NextSongCmd

        private RelayCommand _nextSongCmd;

        public ICommand NextSongCmd
        {
            get { return _nextSongCmd ?? (_nextSongCmd = new RelayCommand(param => NextSongExecute(param as bool?))); }
        }

        private void NextSongExecute(bool? isEnded = false)
        {
            IsDownlading = false;
            DownloadProgress = 0;
            //If song is ended, add it to history(Display inverted order)
            if (isEnded.GetValueOrDefault())
            {
                HistorySongList.Insert(0, CurrentSong);
                if (Account.AccountInfo != null)
                {
                    var para = new SongActionParameter(Account.AccountInfo)
                        .CurrentSongID(CurrentSong)
                        .CurrentChennal(CurrentChannel)
                        .PositionSeconds(CurrentSong.Length);
                    Task.Run(() => SongService.CompletedSong(para));
                }
            }

            Action<List<Song>> action = songs =>
                {
                    if (songs != null) songs.ForEach(s => SongList.Add(s));
                    if (SongList.Count < 1) return;
                    //Set current song & playing
                    CurrentSong = SongList[0];
                    //Play current new song with new url
                    MediaManager.StartPlayerCmd.Execute(null);
                    //RaisePropertyChanged("MediaManager");
                    SongList.RemoveAt(0);
                };
            //Can only play after get song list
            if (SongList.Count < 1) GetSongList(action);
            //Directlly play next & get song list in another thread when list count less then elements
            else if (SongList.Count < 3)
            {
                //Task to get song list
                GetSongList(songs => songs.ForEach(s => SongList.Add(s)));
                //Play
                action(null);
            }
            //Directlly play next
            else action(null);
        }

        #endregion
        
        #region RelayCommand LikeSongCmd

        private RelayCommand _likeSongCmd;

        public ICommand LikeSongCmd
        {
            get { return _likeSongCmd ?? (_likeSongCmd = new RelayCommand(param => LikeSongExecute(param as string))); }
        }

        private void LikeSongExecute(string isHate)
        {
            OperationType actionType;
            var ishate = !string.IsNullOrWhiteSpace(isHate);
            if (ishate)
            {
                actionType = OperationType.Hate;
                NextSongCmd.Execute(false);
            }
            else
                actionType = CurrentSong.Like == 0 ? OperationType.Like : OperationType.DisLike;

            MediaManager.IsBuffering = true;
            Action<List<Song>> action = songs =>
            {
                SongList.Clear();
                if (!ishate && songs.Count > 0)
                    CurrentSong.Like = CurrentSong.Like == 0 ? 1 : 0;
                songs.ForEach(s => SongList.Add(s));
                MediaManager.IsBuffering = false;
            };
            GetSongList(action, actionType);
        }
        #endregion  

        #region RelayCommand ToggleLyricDisplayCmd

        private RelayCommand _toggleLyricDisplayCmd;

        public ICommand ToggleLyricDisplayCmd
        {
            get{ return _toggleLyricDisplayCmd ?? (_toggleLyricDisplayCmd = new RelayCommand(s => ToggleLyricDisplayExecute()));}
        }

        private void ToggleLyricDisplayExecute()
        {
            IsDisylayLyric = !IsDisylayLyric;
        }
        #endregion

        #region RelayCommand HideLoginBoxCmd

        private RelayCommand _hideLoginBoxCmd;

        public ICommand HideLoginBoxCmd
        {
            get { return _hideLoginBoxCmd ?? (_hideLoginBoxCmd = new RelayCommand(s => HideLoginBoxExecute())); }
        }

        private void HideLoginBoxExecute()
        {
            if (string.IsNullOrEmpty(Account.UserName) || Account.AccountInfo == null)
                Account.UserName = LocalTextHelper.GetLocText("Login");
            Account.IsShowLoginBox = false;
            Account.Feedback = string.Empty;
            if (Account.AccountInfo != null && Account.UserName != Account.AccountInfo.UserName)
                Account.UserName = Account.AccountInfo.UserName;
        }

        #endregion

        #region RelayCommand SetChannelCmd

        private RelayCommand _setChannelCmd;

        public ICommand SetChannelCmd
        {
            get { return _setChannelCmd ?? (_setChannelCmd = new RelayCommand(param => SetChannelExecute(param as int?))); }
        }

        private void SetChannelExecute(int? cid)
        {
            var id = cid.GetValueOrDefault();
            CurrentChannel = Channels.FirstOrDefault(s => s.Id == id);
            SongList.Clear();
            NextSongCmd.Execute(false);
            IsShowPlayerDetail = false;
        }
        #endregion

        #region RelayCommand DownloadSongCmd

        private RelayCommand _downloadSongCmd;

        public ICommand DownloadSongCmd
        {
            get { return _downloadSongCmd ?? (_downloadSongCmd = new RelayCommand(param => DownloadSongExetute())); }
        }

        private void DownloadSongExetute()
        {
            IsDownlading = true;
            DownloadProgress = 0;
            var name = CurrentSong.Artist + "-" + CurrentSong.Title + ".mp3";

            //TODO: Add this to setting pannel
            var folder = SettingHelper.GetSetting("DownloadFolder", App.Name);
            if (string.IsNullOrWhiteSpace(folder))
            {
                folder = Environment.CurrentDirectory + "\\DownloadSongs\\";
                SettingHelper.SetSetting("DownloadFolder", folder, App.Name);
            }
            var list = new List<string>(MediaManager.Lyric.Mp3Urls) {CurrentSong.Url};
            HttpWebDealer.DownloadLargestFile(name, list, folder, DownloadMonitor);
        }

        private void DownloadMonitor(object webClient, DownloadProgressChangedEventArgs e)
        {
            DownloadProgress = e.ProgressPercentage;
            if (DownloadProgress == 100) IsDownlading = false;
        }
        #endregion
        
        #region RelayCommand OpenDownloadFolderCmd

        private RelayCommand _openDownloadFolderCmd;

        public ICommand OpenDownloadFolderCmd
        {
            get { return _openDownloadFolderCmd ?? (_openDownloadFolderCmd = new RelayCommand(s => OpenDownloadFolderExecute())); }
        }

        private void OpenDownloadFolderExecute()
        {
            Task.Run(() =>
                {
                    var folder = SettingHelper.GetSetting("DownloadFolder", App.Name);
                    var name = CurrentSong.Artist + "-" + CurrentSong.Title + ".mp3";
                    var path = folder.EndsWith(@"\") ? folder + name : folder + "\\" + name;
                    Process.Start("Explorer.exe", "/select," + path);
                });
        }
        #endregion

        #region RelayCommand OpenSettingWindowCmd

        private RelayCommand _openSettingWindowCmd;

        public ICommand OpenSettingWindowCmd
        {
            get { return _openSettingWindowCmd ?? (_openSettingWindowCmd = new RelayCommand(s => OpenSettingWindowExecure())); }
        }

        private void OpenSettingWindowExecure() 
        {
            if (SettingWindow.IsOpened) return;
            var wd = new SettingWindow(this) {Owner = MainWindow};
            wd.Show();
        }
        #endregion  

        #region RelayCommand ToggleDesktopLyricCmd

        private RelayCommand _toggleDesktopLyricCmd;

        public ICommand ToggleDesktopLyricCmd
        {
            get { return _toggleDesktopLyricCmd ?? (_toggleDesktopLyricCmd = new RelayCommand(s => ToggleDesktopLyricExecute())); }
        }

        private void ToggleDesktopLyricExecute()
        {
            if (DesktopLyric.IsOpened)
            {
                var ownedWds = MainWindow.OwnedWindows;
                for (var i = 0; i < ownedWds.Count; i++)
                {
                    var wd = ownedWds[i];
                    if (!(wd is DesktopLyric)) continue;
                    wd.Close();
                    return;
                }
            }
            else
            {
                var wd = new DesktopLyric(this) { Owner = MainWindow };
                wd.Show();
            }
        }
        #endregion

        #region RelayCommand ChangeSongServiceCmd

        private RelayCommand _changeSongServiceCmd;

        public ICommand ChangeSongServiceCmd
        {
            get { return _changeSongServiceCmd ?? (_changeSongServiceCmd = new RelayCommand(s => ChangeSongService(s as Type))); }
        }

        private void ChangeSongService(Type type) 
        {
            if (type == null || SongService.GetType() == type) return;
            //SongService = (ISongService)Activator.CreateInstance(type);
            SongService = AvalibleSongServices.First(s => s.GetType() == type);
            GetChannels();
        }

        #endregion

        #endregion

        #region Construct Method
        /// <summary>
        /// Please call the GetInstance method
        /// </summary>
        /// <param name="window"></param>
        private MainViewModel(MainWindow window)
        {
            MainWindow = window;

            MediaManager = new MediaManager(this);
            Account = new AccountManager(this);
            WeatherMgr = new WeatherManager(this);
            
            ComposeSongService();
            if (AvalibleSongServices == null || AvalibleSongServices.Count < 1) return;
            //TODO: Change this to Setting center
            SongService = AvalibleSongServices.First(s => s is DoubanFm);

            StartPlayer();
        }

        public static MainViewModel GetInstance(MainWindow window = null)
        {
            return _instance ?? (_instance = new MainViewModel(window));
        }
        #endregion

        #region Processors

        private void StartPlayer()
        {
            GetChannels();
            var songListExpired = SettingHelper.GetSetting(SongListExpireCacheName, App.Name).Deserialize<DateTime>();
            var songList = songListExpired < DateTime.Now
                               ? new List<Song>()
                               : SettingHelper.GetSetting(SongListCacheName, App.Name).Deserialize<List<Song>>();
            if (songList != null && songList.Count > 0)
            {
                songList.ForEach(s => SongList.Add(s));
                NextSongCmd.Execute(false);
            }
            else if (CurrentChannel != null)
                SetChannelCmd.Execute(CurrentChannel.Id);
        }

        private void GetChannels()
        {
            var basicChannels = SongService.GetChannels();
            if (Channels != null) Channels.Clear();
            Channels = Channels ?? new ObservableCollection<Channel>();
            basicChannels.ForEach(s => Channels.Add(s));
            if (CurrentChannel == null) CurrentChannel = Channels.FirstOrDefault();

            Task.Run(() =>
            {
                var allChannels = SongService.GetChannels(false);
                var crtChannel = allChannels.FirstOrDefault(s => s.Id == CurrentChannel.Id && s.StrId == CurrentChannel.StrId);
                MainWindow.Dispatcher.InvokeAsync(() =>
                {
                    Channels.Clear();
                    allChannels.ForEach(s => Channels.Add(s));
                    if (CurrentChannel != null)
                        CurrentChannel = crtChannel ?? CurrentChannel;
                    else
                        CurrentChannel = Channels.FirstOrDefault();
                });
            });
        }

        /// <summary>
        /// Get Song List
        /// </summary>
        /// <param name="callBack">Call Back Function(para is song list get from server)</param>
        /// <param name="actionType">OperationType (Played is default)</param>
        private void GetSongList(Action<List<Song>> callBack = null, OperationType actionType = OperationType.Played)
        {
            Task.Run(() =>
            {
                MediaManager.IsBuffering = true;
                //Get existed songs id list
                var exitingIds = SongList.Select(s => s.Sid);
                //Generate gain song parameter
                var para = new GainSongParameter(Account.AccountInfo)
                    .HistoryString(new List<Song>(HistorySongList) { CurrentSong }, actionType)
                    .PositionSeconds((int)MediaManager.Position.TotalSeconds)
                    .CurrentSongID(CurrentSong)
                    .CurrentChennal(CurrentChannel);
                var songs = SongService.GetSongList((GainSongParameter)para).Where(s => !exitingIds.Contains(s.Sid)).ToList();

                //Notify song list back to the main thread
                MainWindow.Dispatcher.InvokeAsync(() => { if (callBack != null) callBack(songs); });
                SettingHelper.SetSetting(SongListCacheName, songs.SerializeToString(), App.Name);
                //The url of song will expired in 2 hours
                SettingHelper.SetSetting(SongListExpireCacheName, DateTime.Now.AddHours(2).SerializeToString(), App.Name);
                MediaManager.IsBuffering = false;
            });
        }

        private void ComposeSongService()
        {
            var catalog = new AssemblyCatalog((typeof(ISongService).Assembly));
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
        #endregion
    }
}