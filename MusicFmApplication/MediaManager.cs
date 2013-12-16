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
using WPFSoundVisualizationLib;

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

        private readonly MainViewModel viewModel;

        private List<TimeSpan> lrcKeys=new List<TimeSpan>();

        private readonly SpectrumPlayer spectrumPlayer;

        public MediaManager(MainViewModel viewModel)
        {
            PausePlayerCommand = new DelegateCommand(PausePlayerExecute);
            StartPlayerCommand = new DelegateCommand(StartPlayerExecute);
            MuteCommand = new DelegateCommand(MuteExecute);

            spectrumPlayer = new SpectrumPlayer(this);

            this.viewModel = viewModel;
            Volume = 0.75;
            PlayerControl();
        }

        private void PlayerControl()
        {
            Player = viewModel.MainWindow.Player;
            Player.MediaOpened += PlayerMediaOpened;
        }

        private void PlayerMediaOpened(object sender, RoutedEventArgs e)
        {
            IsPlaying = true;
            lrcKeys = viewModel.Lyric.Content.Keys.ToList();
            viewModel.MainWindow.LrcContaner.ScrollToTop();

            var player = (MediaElement)sender;
            SongLength = player.NaturalDuration.TimeSpan;

            //viewModel.MainWindow.SpectrumAnalyzer.RegisterSoundPlayer(spectrumPlayer);

            var timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(300)};
            timer.Tick += TimeTick;
            timer.Start();
        }

        private void TimeTick(object sender, EventArgs e)
        {
            Position = Player.Position;
            DownloadProgress = Player.DownloadProgress;
            viewModel.IsBuffering = Player.IsBuffering;
            PlayProgress = Position.TotalMilliseconds/SongLength.TotalMilliseconds;
            //Song is almost finish, jump to next one
            if ((SongLength.TotalMilliseconds - Position.TotalMilliseconds) < 100)
            {
                viewModel.NextSongCommand.Execute(true);
                return;
            }
            //Lrc control
            var lyricCount = viewModel.Lyric.Content.Count;
            if (lyricCount < 2) return;
            if (lrcKeys.Count != lyricCount) lrcKeys = viewModel.Lyric.Content.Keys.ToList();
            var nextIndex = viewModel.CurrnetLrcLine.Key + 1;
            if (nextIndex >= lrcKeys.Count) return;

            var nextTime = lrcKeys[nextIndex];
            if (Position.TotalMilliseconds > nextTime.TotalMilliseconds + viewModel.Lyric.Offset)
            {
                viewModel.CurrnetLrcLine = new KeyValuePair<int, TimeSpan>(nextIndex, nextTime);
                //not scroll at first 4 line
                if (nextIndex < 4) return;
                viewModel.MainWindow.LrcContaner.LineDown();
                //Scroll two lines every 3 times
                if (nextIndex % 3 == 1)
                    viewModel.MainWindow.LrcContaner.LineDown();
            }
        }
    }


    internal class SpectrumPlayer:ISpectrumPlayer
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private readonly MediaManager _mediaMgr;
        public SpectrumPlayer(MediaManager mediaMgr)
        {
            _mediaMgr = mediaMgr;
        }

        public bool GetFFTData(float[] fftDataBuffer)
        {
            throw new NotImplementedException();
        }

        public int GetFFTFrequencyIndex(int frequency)
        {
            throw new NotImplementedException();
        }

        public bool IsPlaying { get { return _mediaMgr.IsPlaying; } }
    }
}
