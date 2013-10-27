using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
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
        #region Notify Properties

        #region Weather (NotificationObject Property)

        private Weahter _weather;

        public Weahter Weather
        {
            get { return _weather; }
            set
            {
                if (_weather != null && _weather.Equals(value)) return;
                _weather = value;
                RaisePropertyChanged("Weather");
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

        #region SongList (INotifyPropertyChanged Property)

        private ObservableCollection<Song> _SongList;

        public ObservableCollection<Song> SongList
        {
            get { return _SongList ?? (_SongList = new ObservableCollection<Song>()); }
            set
            {
                if (_SongList != null && _SongList.Equals(value)) return;
                _SongList = value;
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

        #endregion

        #region Private Properties

        private static MainViewModel _instance;

        #endregion

        #region Delegate Commands

        #region GetWeatherDetail DelegateCommand
        public DelegateCommand GetWeatherDetailCommmand { get; private set; }
        private void GetWeatherDetailExecute()
        {
            IsShowWeatherDetail = !IsShowWeatherDetail;
        }
        #endregion

        public DelegateCommand NextSongCommand { get; private set; }
        private void NextSongExecute() 
        {
            var index = SongList.IndexOf(CurrentSong) + 1;
            //Get song list before last 2 songs
            if (index+2 >= SongList.Count) GetSongs();
            if (index >= SongList.Count) return;
            CurrentSong = SongList[index];
            if (!MediaManager.IsPlaying)
                MediaManager.StartPlayerCommand.Execute();
        }

        public DelegateCommand ToggleLyricDisplayCommand { get; private set; }
        private void ToggleLyricDisplayExecute() 
        {
            IsDisylayLyric = !IsDisylayLyric;
        }

        #endregion

        public MainWindow MainWindow { get; private set; }

        #region Construct Method
        /// <summary>
        /// Please call the GetInstance method
        /// </summary>
        /// <param name="window"></param>
        private MainViewModel(MainWindow window)
        {
            MainWindow = window;
            GetWeatherDetailCommmand = new DelegateCommand(GetWeatherDetailExecute);
            NextSongCommand = new DelegateCommand(NextSongExecute);
            ToggleLyricDisplayCommand = new DelegateCommand(ToggleLyricDisplayExecute);

            GetWeather();

            //Change this with MEF
            SongService = new DoubanFm();
            GetSongs();

        }

        public static MainViewModel GetInstance(MainWindow window=null) 
        {
            return _instance ?? (_instance = new MainViewModel(window));
        }
        #endregion

        #region GetWeather
        /// <summary>
        /// Get weather info
        /// </summary>
        private void GetWeather()
        {
            Task.Factory.StartNew(() =>
            {
                Weather = CityWeatherHelper.GetWeather();

                if (Weather.LifeIndexes != null && Weather.LifeIndexes.Count > 0 &&
                    !(Weather.LifeIndexes is ObservableCollection<LifeIndex>))
                {
                    Weather.LifeIndexes = new ObservableCollection<LifeIndex>(Weather.LifeIndexes);
                }
            });
        }
        #endregion

        private void GetSongs()
        {
            IsGettingSong = true;
            Task.Factory.StartNew(() =>
            {
                var exitingIds = SongList.Select(s => s.Sid);
                var songs = SongService.GetSongList().Where(s => !exitingIds.Contains(s.Sid)).ToList();
                if (!songs.Any())
                {
                    GetSongs();
                    return;
                }

                MainWindow.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        songs.ForEach(s => SongList.Add(s));
                        if (CurrentSong == null)
                        {
                            CurrentSong = SongList.First();
                            MediaManager.StartPlayerCommand.Execute();
                        }
                        IsGettingSong = false;
                    }));
            });
        }

    }
}
