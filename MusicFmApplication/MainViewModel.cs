using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using CommonHelperLibrary;
using CommonHelperLibrary.WEB;
using CustomControlResources;
using MahApps.Metro.Controls;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Practices.Prism.Commands;
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
    public class MainViewModel : NotificationObject
    {
        #region Fields

        private static MainViewModel _instance;

        public MainWindow MainWindow { get; private set; }

        public string AppName = "MusicFM";

        #endregion

        #region Notify Properties

        #region MediaManager (INotifyPropertyChanged Property)

        private MediaManager _mediaManager;

        public MediaManager MediaManager
        {
            get { return _mediaManager ?? (_mediaManager = new MediaManager(this)); }
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
            get { return _account ?? (_account = new AccountManager(this)); }
            private set
            {
                if (_account != null && _account.Equals(value)) return;
                _account = value;
                RaisePropertyChanged("Account");
            }
        }

        #endregion

        #region IsShowWeatherDetail (NotificationObject Property)
        private bool _isShowWeatherDetail;

        public bool IsShowWeatherDetail
        {
            get { return _isShowWeatherDetail; }
            set
            {
                if (_isShowWeatherDetail.Equals(value)) return;
                _isShowWeatherDetail = value;
                RaisePropertyChanged("IsShowWeatherDetail");
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

        #region IsBuffering (INotifyPropertyChanged Property)

        private bool _isBuffering;

        public bool IsBuffering
        {
            get { return _isBuffering; }
            set
            {
                if (_isBuffering.Equals(value)) return;
                _isBuffering = value;
                RaisePropertyChanged("IsBuffering");
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

        #region Lyric (INotifyPropertyChanged Property)

        private SongLyric _lyric;

        public SongLyric Lyric
        {
            get { return _lyric; }
            set
            {
                if (_lyric != null && _lyric.Equals(value)) return;
                _lyric = value;
                RaisePropertyChanged("Lyric");
            }
        }

        #endregion

        #region CurrnetLrcLine (INotifyPropertyChanged Property)

        private KeyValuePair<int, TimeSpan> _CurrnetLrcLine;

        public KeyValuePair<int, TimeSpan> CurrnetLrcLine
        {
            get { return _CurrnetLrcLine; }
            set
            {
                if (_CurrnetLrcLine.Equals(value)) return;
                _CurrnetLrcLine = value;
                RaisePropertyChanged("CurrnetLrcLine");
            }
        }

        #endregion

        #region Channels (INotifyPropertyChanged Property)

        private AsyncProperty<ObservableCollection<Channel>> _channels;

        public AsyncProperty<ObservableCollection<Channel>> Channels
        {
            get { return _channels; }
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

        #region ShowWeatherDetail DelegateCommand
        public DelegateCommand ShowWeatherDetailCommmand { get; private set; }
        private void ShowWeatherDetailExecute()
        {
            IsShowWeatherDetail = !IsShowWeatherDetail;
        }
        #endregion

        public DelegateCommand TogglePlayerDetailCommand { get; private set; }
        private void TogglePlayerDetailExecute() 
        {
            IsShowPlayerDetail = !IsShowPlayerDetail;

            if (string.IsNullOrEmpty(Account.UserName) || Account.AccountInfo == null) 
                Account.UserName = LocalTextHelper.GetLocText("LoginDouban");
            Account.IsShowLoginBox = false;
            Account.Feedback = string.Empty;
            if (Account.AccountInfo != null && Account.UserName != Account.AccountInfo.UserName)
                Account.UserName = Account.AccountInfo.UserName;
        }

        public DelegateCommand<bool?> NextSongCommand { get; private set; }
        private void NextSongExecute(bool? isEnded = false)
        {
            IsBuffering = true;
            IsDownlading = false;
            DownloadProgress = 0;
            //If song is ended, add it to history(Display inverted order)
            if (isEnded.GetValueOrDefault())
            {
                HistorySongList.Insert(0, CurrentSong);
                if (Account.AccountInfo != null)
                {
                    var para = CreateGetSongParamter("", false);
                    Task.Run(() => SongService.CompletedSong(para));
                }
            }

            Action action = () =>
            {
                //Set current song & playing
                CurrentSong = SongList[0];
                //Get song lyric
                GetLyric();
                //Play current new song with new url
                if (!MediaManager.IsPlaying)
                    MediaManager.StartPlayerCommand.Execute();
                SongList.RemoveAt(0);
            };
            //At first, only can play after get song list
            if (SongList.Count < 1) GetSongList(action);
            //Directlly play next & get song list in another thread when list count less then elements
            else if (SongList.Count < 3)
            {
                GetSongList();
                action();
            }
            //Directlly play next
            else action();
        }

        public DelegateCommand<string> LikeSongCommand { get; private set; }
        public void LikeSongExecute(string isHate)
        {
            string action;
            var ishate = !string.IsNullOrWhiteSpace(isHate);
            if (ishate)
            {
                action = ":s|";

                //Play Next Song
                CurrentSong = SongList[0];
                GetLyric();
                MediaManager.StartPlayerCommand.Execute();
                SongList.RemoveAt(0);
            }
            else action = CurrentSong.Like == 0 ? ":r|" : ":u|";

            var para = CreateGetSongParamter(action);
            IsBuffering = true;
            Task.Factory.StartNew(() =>
                {
                    var songs = SongService.GetSongList(para).ToList();
                    if (songs.Count < 1) return;
                    //Notify song list back to the main thread
                    MainWindow.Dispatcher.BeginInvoke((Action) (() =>
                        {
                            SongList.Clear();
                            if (!ishate)
                                CurrentSong.Like = CurrentSong.Like == 0 ? 1 : 0;
                            songs.ForEach(s => SongList.Add(s));
                        }));
                    IsBuffering = false;
                });
        }

        public DelegateCommand ToggleLyricDisplayCommand { get; private set; }
        private void ToggleLyricDisplayExecute() 
        {
            IsDisylayLyric = !IsDisylayLyric;
        }

        public DelegateCommand<int?> SetChannelCommand { get; private set; }
        private void SetChannelExecute(int? cid) 
        {
            var id = cid.GetValueOrDefault();
            CurrentChannel = Channels.AsyncValue.FirstOrDefault(s => s.Id == id);
            SongList.Clear();
            NextSongCommand.Execute(false);
            IsShowPlayerDetail = false;
        }

        public DelegateCommand DownloadSongCommand { get; private set; }
        private void DownloadSongExetute()
        {
            IsDownlading = true;
            DownloadProgress = 0;
            var name = CurrentSong.Artist + "-" + CurrentSong.Title + ".mp3";
            var folder = SettingHelper.GetSetting("DownloadFolder", AppName);
            HttpWebDealer.DownloadLargestFile(name, Lyric.Mp3Urls, folder, DownloadMonitor);
        }

        private void DownloadMonitor(object webClient, DownloadProgressChangedEventArgs e)
        {
            DownloadProgress = e.ProgressPercentage;
            if (DownloadProgress == 100) IsDownlading = false;
        }

        public DelegateCommand OpenDownloadFolderCommand { get; private set; }
        public void OpenDownloadFolderExecute()
        {
            Task.Run(() =>
                {
                    var folder = SettingHelper.GetSetting("DownloadFolder", AppName);
                    var name = CurrentSong.Artist + "-" + CurrentSong.Title + ".mp3";
                    var path = folder.EndsWith(@"\") ? folder + name : folder + "\\" + name;
                    System.Diagnostics.Process.Start("Explorer.exe", "/select," + path);
                });
        }

        #endregion

        #region Construct Method
        /// <summary>
        /// Please call the GetInstance method
        /// </summary>
        /// <param name="window"></param>
        private MainViewModel(MainWindow window)
        {
            MainWindow = window;
            ShowWeatherDetailCommmand = new DelegateCommand(ShowWeatherDetailExecute);
            NextSongCommand = new DelegateCommand<bool?>(NextSongExecute);
            LikeSongCommand = new DelegateCommand<string>(LikeSongExecute);
            ToggleLyricDisplayCommand = new DelegateCommand(ToggleLyricDisplayExecute);
            TogglePlayerDetailCommand=new DelegateCommand(TogglePlayerDetailExecute);
            SetChannelCommand = new DelegateCommand<int?>(SetChannelExecute);
            DownloadSongCommand=new DelegateCommand(DownloadSongExetute);
            OpenDownloadFolderCommand = new DelegateCommand(OpenDownloadFolderExecute);

            //TODO: Change this with MEF
            SongService = new DoubanFm();
            //TODO: Change this to setting pannel
            Task.Run(() =>
                {
                    if (!string.IsNullOrWhiteSpace(SettingHelper.GetSetting("DownloadFolder", AppName))) return;
                    var folder = Environment.CurrentDirectory + "\\DownloadSongs\\";
                    SettingHelper.SetSetting("DownloadFolder", folder, AppName);
                });

            GetChannels();
            Account.GerAccountFromConfig();
        }

        public static MainViewModel GetInstance(MainWindow window=null) 
        {
            return _instance ?? (_instance = new MainViewModel(window));
        }
        #endregion

        #region Processors
        private void GetChannels()
        {
            var basicChannels = SongService.GetChannels();
            var task = new Func<Task<ObservableCollection<Channel>>>(() => Task.Run(() => SongService.GetChannels(false)));
            Channels = new AsyncProperty<ObservableCollection<Channel>>(task, basicChannels);
            var first = basicChannels.FirstOrDefault();
            if (first != null)
                SetChannelCommand.Execute(first.Id);
        }

        /// <summary>
        /// Get Song List
        /// </summary>
        /// <param name="callBack">Call Back Function</param>
        private void GetSongList(Action callBack = null)
        {
            IsBuffering = true;
            Task.Factory.StartNew(() =>
            {
                //Get Song List
                var exitingIds = SongList.Select(s => s.Sid);
                var para = CreateGetSongParamter();
                var songs = SongService.GetSongList(para).Where(s => !exitingIds.Contains(s.Sid)).ToList();
                //Notify song list back to the main thread
                MainWindow.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        songs.ForEach(s => SongList.Add(s));
                        if (callBack != null) callBack();
                    }));
                IsBuffering = false;
            });
        }

        private void GetLyric()
        {
            Lyric = new SongLyric
            {
                Title = CurrentSong.Title,
                Album = CurrentSong.AlbumTitle,
                Artist = CurrentSong.Artist
            };
            Lyric.Mp3Urls.Add(CurrentSong.Url);
            Lyric.Content.Add(new TimeSpan(0), "Trying to Get Lyrics, Please wait");
            Task.Factory.StartNew(() =>
                {
                    var lrc = SongLyricHelper.GetSongLyric(CurrentSong.Title, CurrentSong.Artist);
                    if (lrc == null || !lrc.Content.Any()) return;
                    MainWindow.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            Lyric = lrc;
                            CurrnetLrcLine = new KeyValuePair<int, TimeSpan>(0, lrc.Content.First().Key);
                        }));
                });
        }



        /// <summary>
        /// Get Song History String to bulid URL
        /// </summary>
        /// <param name="action">Played: ":p|"  Like: ":r|"  Unlike: ":u|"  Hate: ":s|"</param>
        /// <param name="needHistory">need history or not(default true)</param>
        /// <returns></returns>
        public GetSongParameter CreateGetSongParamter(string action = ":p|", bool needHistory = true)
        {
            var sb = new StringBuilder();
            if (needHistory)
            {
                //History can add 20 songs at most
                var historyCount = HistorySongList.Count > 19 ? 19 : HistorySongList.Count;
                for (var i = 0; i < historyCount; i++)
                    sb.Append(HistorySongList[i].Sid + ":p|");
                if (CurrentSong != null) sb.Append(CurrentSong.Sid + action);
            }

            var para = new GetSongParameter
                {
                    ChannelId = CurrentChannel == null ? 0 : CurrentChannel.Id,
                    SongId = CurrentSong == null ? "0" : CurrentSong.Sid.ToString()
                };
            if (Account.AccountInfo != null)
            {
                para.History = sb.ToString();
                para.UserId = Account.AccountInfo.UserId;
                para.Token = Account.AccountInfo.Token;
                para.Expire = Account.AccountInfo.ExpireString;
            }
            return para;
        }
        #endregion

    }
}
