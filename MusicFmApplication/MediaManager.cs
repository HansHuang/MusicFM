using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
            if (Player == null) return;
            Player.Play();
            IsPlaying = true;
        }


        public DelegateCommand MuteCommand { get; private set; }
        private void MuteExecute() {
            if (Volume > 0) {
                _volumeCache = Volume;
                Volume = 0;
            }
            else
                Volume = _volumeCache;
        }

        #endregion

        public MediaElement Player { get; private set; }

        private readonly MainViewModel ViewModel;

        public MediaManager(MainViewModel viewModel) 
        {
            ViewModel = viewModel;
            Volume = 0.8;
            PlayerControl();

            PausePlayerCommand = new DelegateCommand(PausePlayerExecute);
            StartPlayerCommand = new DelegateCommand(StartPlayerExecute);
            MuteCommand = new DelegateCommand(MuteExecute);
        }

        private void PlayerControl() {
            Player = ViewModel.MainWindow.Player;
            Player.MediaOpened += PlayerMediaOpened;
        }

        private void PlayerMediaOpened(object sender, RoutedEventArgs e)
        {
            IsPlaying = true;
            var player = (MediaElement)sender;
            SongLength = player.NaturalDuration.TimeSpan;
            var timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(300)};
            timer.Tick += TimeTick;
            timer.Start();
        }

        private void TimeTick(object sender, EventArgs e)
        {
            Position = Player.Position;
            DownloadProgress = Player.DownloadProgress;
            PlayProgress = Position.TotalMilliseconds/SongLength.TotalMilliseconds;
            if ((SongLength.TotalMilliseconds - Position.TotalMilliseconds) < 300)
                ViewModel.NextSongCommand.Execute();
            ViewModel.IsGettingSong = Player.IsBuffering;
        }
    }
}
