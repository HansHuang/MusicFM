using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using CommonHelperLibrary;
using CommonHelperLibrary.WEB;
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

        #endregion

        #region Notify Properties

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

        #region IsGettingSong (INotifyPropertyChanged Property)

        private bool _isGettingSong;

        public bool IsGettingSong
        {
            get { return _isGettingSong; }
            set
            {
                if (_isGettingSong.Equals(value)) return;
                _isGettingSong = value;
                RaisePropertyChanged("IsGettingSong");
            }
        }

        #endregion

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

        #region CurrnetLrcLine (INotifyPropertyChanged Property)

        private KeyValuePair<int,TimeSpan> _CurrnetLrcLine;

        public KeyValuePair<int,TimeSpan> CurrnetLrcLine
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
        }

        public DelegateCommand<bool?> NextSongCommand { get; private set; }
        private void NextSongExecute(bool? isEnded = false)
        {
            //If last song is ended, add it to history
            if (isEnded.GetValueOrDefault()) HistorySongList.Add(CurrentSong);

            Action action = () =>
            {
                //Set current song & playing
                CurrentSong = SongList[0];
                //Get song lyric
                GetLyric();
                if (!MediaManager.IsPlaying) MediaManager.StartPlayerCommand.Execute();
                SongList.RemoveAt(0);
            };
            if (SongList.Count == 0)
                GetSongList(CurrentChannel.Id, action);
            else if (SongList.Count < 3)
            {
                GetSongList(CurrentChannel.Id);
                action();
            }
            else
                action();
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
            ToggleLyricDisplayCommand = new DelegateCommand(ToggleLyricDisplayExecute);
            TogglePlayerDetailCommand=new DelegateCommand(TogglePlayerDetailExecute);
            SetChannelCommand = new DelegateCommand<int?>(SetChannelExecute);

            //Change this with MEF
            SongService = new DoubanFm();

            GetChannels();
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

        private void GetSongList(int cid = 0, Action callBack = null)
        {
            IsGettingSong = true;
            Task.Factory.StartNew(() =>
            {
                //Get Song List
                var exitingIds = SongList.Select(s => s.Sid);
                var para = new GetSongParameter { ChannelId = cid };
                var songs = SongService.GetSongList(para).Where(s => !exitingIds.Contains(s.Sid)).ToList();
                //Notify song list back to the main thread
                MainWindow.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        songs.ForEach(s => SongList.Add(s));
                        if (callBack != null) callBack();
                    }));
                IsGettingSong = false;
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
        #endregion

    }
}
