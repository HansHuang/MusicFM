using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CommonHelperLibrary;
using CommonHelperLibrary.WEB;
using CustomControlResources;
using Color = System.Windows.Media.Color;

namespace MusicFmApplication
{
    public class MediaManager : INotifyPropertyChanged
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
        public MediaPlayer Player { get; private set; }

        private List<TimeSpan> _lyricKeys;
        public List<TimeSpan> LrcKeys
        {
            get { return _lyricKeys ?? (_lyricKeys = new List<TimeSpan>()); }
            private set { _lyricKeys = value; }
        }

        protected readonly MainViewModel ViewModel;
        protected DispatcherTimer Timer;

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

        protected const string VolumeCacheName = "Volume";
        private double _volumeCache;
        private double _volume = 0.75;
        public double Volume
        {
            get { return _volume; }
            set
            {
                if (_volume.Equals(value)) return;
                _volume = value;
                if (Player != null) Player.Volume = value;
                SettingHelper.SetSetting(VolumeCacheName, value.SerializeToString(), App.Name);
                RaisePropertyChanged("Volume");
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
                if (value != null && value.Content != null)
                {
                    LrcKeys = value.Content.Keys.ToList();
                    if (value.Content.Count > 0)
                        CurrnetLrcLine = new KeyValuePair<int, TimeSpan>(0, _lyric.Content.First().Key);
                }
                else
                    LrcKeys = new List<TimeSpan>();
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
        private BitmapSource _songPicture;
        public BitmapSource SongPicture
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

        #region RelayCommand PauseResumePlayerCmd

        private RelayCommand _pauseResumePlayerCmd;

        public ICommand PauseResumePlayerCmd
        {
            get { return _pauseResumePlayerCmd ?? (_pauseResumePlayerCmd = new RelayCommand(s => PauseResumePlayerExecute())); }
        }

        private void PauseResumePlayerExecute()
        {
            if (Player == null || !Player.CanPause) return;
            if (IsPlaying)
            {
                Player.Pause();
                IsPlaying = false;
            }
            else if (ViewModel.CurrentSong != null)
            {
                Player.Play();
                IsPlaying = true;
            }
        }

        #endregion

        #region RelayCommand StartPlayerCmd

        private RelayCommand _startPlayerCmd;

        public ICommand StartPlayerCmd
        {
            get { return _startPlayerCmd ?? (_startPlayerCmd = new RelayCommand(s => StartPlayerExecute())); }
        }

        private void StartPlayerExecute()
        {
            if (Player == null || ViewModel.CurrentSong == null) return;
            
            Player.Open(new Uri(ViewModel.CurrentSong.Url));
            Player.Play();
            IsPlaying = true;
        }

        #endregion

        #region RelayCommand MutePlayerCmd

        private RelayCommand _mutePlayerCmd;

        public ICommand MutePlayerCmd
        {
            get { return _mutePlayerCmd ?? (_mutePlayerCmd = new RelayCommand(s => this.MuteExecute())); }
        }
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

        #endregion

        public MediaManager(MainViewModel viewModel)
        {
            ViewModel = viewModel;

            viewModel.MainWindow.Dispatcher.Invoke(() =>
            {
                Player = new MediaPlayer();
                Player.MediaOpened += PlayerMediaOpened;

                var vlm = SettingHelper.GetSetting(VolumeCacheName, App.Name).Deserialize<double>();
                _volume = vlm < 0.1 ? 0.75 : vlm;
                Player.Volume = _volume;
            });
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
            SongPicture = null;
            var picUrl = ViewModel.CurrentSong.Picture;
            if (string.IsNullOrWhiteSpace(picUrl)) return;
            Task.Run(() =>
            {
                var stream = HttpWebDealer.GetResponseByUrl(picUrl).GetResponseStream();
                if (stream == null) return;
                var bitmap = new Bitmap(stream);
                var image = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                                                                  BitmapSizeOptions.FromEmptyOptions());
                if (image.CanFreeze) image.Freeze();
                DispatcherInvokeAsync(() => { SongPicture = image; });
                var color = ImageColorHelper.GetTopicColorForImage(image);
                DispatcherInvokeAsync(() => { SongPictureColor = color; });
            });
        }

        #region Processors
        private void PlayerMediaOpened(object sender, EventArgs e)
        {
            App.Log.Msg(DateTime.Now + ", Played", ViewModel.CurrentSong.Url);
            IsPlaying = true;
            if (Lyric == null) return;
            LrcKeys = Lyric.Content.Keys.ToList();

            var player = (MediaPlayer)sender;
            SongLength = player.NaturalDuration.HasTimeSpan
                             ? player.NaturalDuration.TimeSpan
                             : new TimeSpan(0, 0, ViewModel.CurrentSong.Length);
            if (Timer == null)
            {
                Timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
                Timer.Tick += TimeTick;
            }
            Timer.Start();
        }

        private void TimeTick(object sender, EventArgs e)
        {
            if (!IsPlaying) return;
            Position = Player.Position;
            DownloadProgress = Player.DownloadProgress;

            //IsBuffering = IsBuffering || Player.IsBuffering;
            PlayProgress = Position.TotalMilliseconds / SongLength.TotalMilliseconds;
            //Song is almost finish, jump to next one
            if ((SongLength.TotalMilliseconds - Position.TotalMilliseconds) < 100)
            {
                Timer.Stop();
                LrcKeys.Clear();
                IsPlaying = false;
                ViewModel.NextSongCmd.Execute(true);
                return;
            }
            //Lrc control
            LyricControl();
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

        private DispatcherOperation DispatcherInvokeAsync(Action callback)
        {
            return ViewModel.MainWindow.Dispatcher.InvokeAsync(callback);
        }

        #endregion
    }
}
