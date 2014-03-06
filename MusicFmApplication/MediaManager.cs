using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;

namespace MusicFmApplication
{
    public class MediaManager : NotificationObject
    {
        #region Notify Properties

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
        private double _volume;
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
            //Task.Run(() =>
            //    {
            //        var image = new BitmapImage(new Uri(ViewModel.CurrentSong.Picture));
            //        image.GetAsFrozen();
            //        ViewModel.MainWindow.Dispatcher.InvokeAsync(() =>
            //            {
            //                ViewModel.MainWindow.AlbumPicture.Source = image;
            //            });
            //    });
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

        public MediaElement Player { get; private set; }

        protected readonly MainViewModel ViewModel;

        protected List<TimeSpan> LrcKeys = new List<TimeSpan>();

        public MediaManager(MainViewModel viewModel)
        {
            PausePlayerCommand = new DelegateCommand(PausePlayerExecute);
            StartPlayerCommand = new DelegateCommand(StartPlayerExecute);
            MuteCommand = new DelegateCommand(MuteExecute);

            ViewModel = viewModel;
            Volume = 0.75;
            PlayerControl();
        }

        private void PlayerControl()
        {
            Player = ViewModel.MainWindow.Player;
            Player.MediaOpened += PlayerMediaOpened;
        }

        private void PlayerMediaOpened(object sender, RoutedEventArgs e)
        {
            IsPlaying = true;
            if (ViewModel.Lyric == null) return;
            LrcKeys = ViewModel.Lyric.Content.Keys.ToList();
            ViewModel.MainWindow.LrcContaner.ScrollToTop();

            var player = (MediaElement)sender;
            if (player.NaturalDuration.HasTimeSpan)
                SongLength = player.NaturalDuration.TimeSpan;
            else
                SongLength = new TimeSpan(0, 0, ViewModel.CurrentSong.Length);
            //viewModel.MainWindow.SpectrumAnalyzer.RegisterSoundPlayer(spectrumPlayer);

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            timer.Tick += TimeTick;
            timer.Start();
        }

        private void TimeTick(object sender, EventArgs e)
        {
            Position = Player.Position;
            DownloadProgress = Player.DownloadProgress;
            ViewModel.IsBuffering = Player.IsBuffering;
            PlayProgress = Position.TotalMilliseconds / SongLength.TotalMilliseconds;
            //Song is almost finish, jump to next one
            if ((SongLength.TotalMilliseconds - Position.TotalMilliseconds) < 100)
            {
                ViewModel.NextSongCommand.Execute(true);
                return;
            }
            //Lrc control
            var lyricCount = ViewModel.Lyric.Content.Count;
            if (lyricCount < 2) return;
            if (LrcKeys.Count != lyricCount) LrcKeys = ViewModel.Lyric.Content.Keys.ToList();
            var nextIndex = ViewModel.CurrnetLrcLine.Key + 1;
            if (nextIndex >= LrcKeys.Count) return;

            var nextTime = LrcKeys[nextIndex];
            if (Position.TotalMilliseconds > nextTime.TotalMilliseconds + ViewModel.Lyric.Offset)
            {
                ViewModel.CurrnetLrcLine = new KeyValuePair<int, TimeSpan>(nextIndex, nextTime);
                //not scroll at first 5 lines
                if (nextIndex < 5) return;
                ViewModel.MainWindow.LrcContaner.LineDown();
                //Scroll two lines every 3 times
                if (nextIndex % 3 == 1)
                    ViewModel.MainWindow.LrcContaner.LineDown();
            }
        }
    }
}
