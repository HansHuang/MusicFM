using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CommonHelperLibrary;
using CommonHelperLibrary.WEB;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;

namespace MusicFmApplication
{
    public class MediaManager : NotificationObject
    {
        #region Fields
        public MediaPlayer Player { get; private set; }

        //public Stream SongImageStream { get; private set; }

        protected readonly MainViewModel ViewModel;

        protected List<TimeSpan> LrcKeys = new List<TimeSpan>();

        protected DispatcherTimer Timer;

        protected BitmapImage SongTempImage;
        protected bool IsInitSongPicture;

        #endregion

        #region Notify Properties

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
        
        #region DownloadProgress (INotifyPropertyChanged Property)

        private double _downloadProgress;

        public double DownloadProgress
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

        #region PlayProgress (INotifyPropertyChanged Property)

        private double _playProgress;

        public double PlayProgress
        {
            get { return _playProgress; }
            set
            {
                if (_playProgress.Equals(value)) return;
                _playProgress = value;
                RaisePropertyChanged("PlayProgress");
            }
        }

        #endregion

        #region Volume (INotifyPropertyChanged Property)

        private double _volumeCache;
        private double _volume = 0.75;
        public double Volume
        {
            get { return _volume; }
            set
            {
                if (_volume.Equals(value)) return;
                _volume = value;
                RaisePropertyChanged("Volume");
                if (Player != null)
                    Player.Volume = value;
            }
        }

        #endregion

        #region Position (INotifyPropertyChanged Property)

        private TimeSpan _position;

        public TimeSpan Position
        {
            get { return _position; }
            set
            {
                if (_position.Equals(value)) return;
                _position = value;
                RaisePropertyChanged("Position");
            }
        }
        #endregion

        #region SongLength (INotifyPropertyChanged Property)

        private TimeSpan _songLength;

        public TimeSpan SongLength
        {
            get { return _songLength; }
            set
            {
                if (_songLength.Equals(value)) return;
                _songLength = value;
                RaisePropertyChanged("SongLength");
            }
        }
        #endregion

        #region IsPlaying (INotifyPropertyChanged Property)

        private bool _isPlaying;

        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                if (_isPlaying.Equals(value)) return;
                _isPlaying = value;
                RaisePropertyChanged("IsPlaying");

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
                if (_lyric != null && _lyric.Content != null && _lyric.Content.Count > 1)
                    CurrnetLrcLine = new KeyValuePair<int, TimeSpan>(0, _lyric.Content.First().Key);
            }
        }

        #endregion

        #region CurrnetLrcLine (INotifyPropertyChanged Property)

        private KeyValuePair<int, TimeSpan> _currnetLrcLine;

        public KeyValuePair<int, TimeSpan> CurrnetLrcLine
        {
            get { return _currnetLrcLine; }
            set
            {
                if (_currnetLrcLine.Equals(value)) return;
                _currnetLrcLine = value;
                RaisePropertyChanged("CurrnetLrcLine");
            }
        }

        #endregion

        #region SongPicture (INotifyPropertyChanged Property)
        private BitmapImage _songPicture;
        public BitmapImage SongPicture
        {
            get { return _songPicture; }
            set
            {
                if (_songPicture != null && _songPicture.Equals(value)) return;
                _songPicture = value;
                RaisePropertyChanged("SongPicture");
            }
        }
        #endregion

        #region SongPictureColor (INotifyPropertyChanged Property)

        private Color _songPictureColor;

        public Color SongPictureColor
        {
            get { return _songPictureColor; }
            set
            {
                if (_songPictureColor.Equals(value)) return;
                _songPictureColor = value;
                RaisePropertyChanged("SongPictureColor");
            }
        }

        #endregion

        #endregion

        #region DelegateCommand

        public DelegateCommand PausePlayerCommand { get; private set; }
        private void PausePlayerExecute()
        {
            if (Player == null || !Player.CanPause) return;
            Player.Pause();
            IsPlaying = false;
        }

        public DelegateCommand StartPlayerCommand { get; private set; }
        private void StartPlayerExecute()
        {
            if (Player == null || ViewModel.CurrentSong == null) return;
            Player.Open(new Uri(ViewModel.CurrentSong.Url));
            Player.Play();
            IsPlaying = true;
        }

        public DelegateCommand MuteCommand { get; private set; }
        private void MuteExecute()
        {
            if (Volume > 0)
            {
                _volumeCache = Volume;
                Volume = 0;
            }
            else
                Volume = _volumeCache <= 0.05 ? (_volumeCache = 0.5) : _volumeCache;
        }

        #endregion

        public MediaManager(MainViewModel viewModel)
        {
            PausePlayerCommand = new DelegateCommand(PausePlayerExecute);
            StartPlayerCommand = new DelegateCommand(StartPlayerExecute);
            MuteCommand = new DelegateCommand(MuteExecute);

            ViewModel = viewModel;
            //Player = ViewModel.MainWindow.Player;
            Player = new MediaPlayer();
            Player.MediaOpened += PlayerMediaOpened;
        }

        /// <summary>
        /// Get lyric of current song
        /// </summary>
        public void GetLyric()
        {
            if (ViewModel.CurrentSong == null) return;
            Lyric = new SongLyric
            {
                Title = ViewModel.CurrentSong.Title,
                Album = ViewModel.CurrentSong.AlbumTitle,
                Artist = ViewModel.CurrentSong.Artist
            };
            Lyric.Mp3Urls.Add(ViewModel.CurrentSong.Url);
            Lyric.Content.Add(new TimeSpan(0), "Trying to get lyric, please wait");
            CurrnetLrcLine = new KeyValuePair<int, TimeSpan>(0, Lyric.Content.First().Key);

            Task.Run(() =>
            {
                var lrc = SongLyricHelper.GetSongLyric(ViewModel.CurrentSong.Title, ViewModel.CurrentSong.Artist);
                if (lrc == null || lrc.Content.Count < 2) return;
                ViewModel.MainWindow.Dispatcher.InvokeAsync(() => { Lyric = lrc; });
            });
        }

        /// <summary>
        /// Get picture of current song
        /// </summary>
        public void GetSongPicture() 
        {
            if (ViewModel.CurrentSong == null) return;
            IsInitSongPicture = true;
            DispatcherInvokeAsync(() =>
            {
                SongTempImage = new BitmapImage();
                SongTempImage.DownloadFailed += (o, e) => { SongTempImage = null; };
                //Something unknown wrong when use new BitmapImage(url) contruction method directly
                SongTempImage.BeginInit();
                SongTempImage.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                SongTempImage.UriSource = new Uri(ViewModel.CurrentSong.Picture);
                SongTempImage.EndInit();
                IsInitSongPicture = false;
            });
        }

        #region Processors
        private void PlayerMediaOpened(object sender, EventArgs e)
        {
            IsPlaying = true;
            if (Lyric == null) return;
            LrcKeys = Lyric.Content.Keys.ToList();

            var player = (MediaPlayer)sender;
            if (player.NaturalDuration.HasTimeSpan)
                SongLength = player.NaturalDuration.TimeSpan;
            else
                SongLength = new TimeSpan(0, 0, ViewModel.CurrentSong.Length);
            if (Timer == null)
            {
                Timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
                Timer.Tick += TimeTick;
            }
            Timer.Start();
        }

        private void TimeTick(object sender, EventArgs e)
        {
            if (!IsPlaying) return;
            Position = Player.Position;
            DownloadProgress = Player.DownloadProgress;
            IsBuffering = Player.IsBuffering;
            PlayProgress = Position.TotalMilliseconds / SongLength.TotalMilliseconds;
            //Song is almost finish, jump to next one
            if ((SongLength.TotalMilliseconds - Position.TotalMilliseconds) < 100)
            {
                Timer.Stop();
                LrcKeys.Clear();
                IsPlaying = false;
                ViewModel.NextSongCommand.Execute(true);
                return;
            }
            //Lrc control
            LyricControl();
            //Song Album Picture Handle
            SongPictureControl();
        }

        private void LyricControl()
        {
            var lyricCount = Lyric.Content.Count;
            if (lyricCount < 2) return;
            if (LrcKeys.Count != lyricCount) LrcKeys = Lyric.Content.Keys.ToList();
            var nextIndex = CurrnetLrcLine.Key + 1;
            if (nextIndex >= LrcKeys.Count) return;

            var nextTime = LrcKeys[nextIndex];
            if (Position.TotalMilliseconds > nextTime.TotalMilliseconds + Lyric.Offset)
                CurrnetLrcLine = new KeyValuePair<int, TimeSpan>(nextIndex, nextTime);
        }

        private void SongPictureControl() 
        {
            if (SongTempImage == null || SongTempImage.IsDownloading || IsInitSongPicture || SongPicture != null)
                return;
            if (SongTempImage.CanFreeze) SongTempImage.Freeze();
            SongPicture = SongTempImage;
            ImageColorHelper.GetTopicColorForImageAsync(SongPicture, (color) => DispatcherInvokeAsync(() => { SongPictureColor = color; }));
        }

        private DispatcherOperation DispatcherInvokeAsync(Action callback)
        {
            return ViewModel.MainWindow.Dispatcher.InvokeAsync(callback);
        }

        #endregion
    }
}
