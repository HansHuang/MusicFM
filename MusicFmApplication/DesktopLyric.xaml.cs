using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MahApps.Metro.Models.Win32;

namespace MusicFmApplication
{
    /// <summary>
    /// Interaction logic for DesktopLyric.xaml
    /// </summary>
    public partial class DesktopLyric : Window, INotifyPropertyChanged
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

        public static bool IsOpened = false;

        public MainViewModel ViewModel { get; private set; }

        protected Storyboard ScrollSmooth;
        
        #region PreviousLine (INotifyPropertyChanged Property)

        private TimeSpan _previousLine;

        public TimeSpan PreviousLine
        {
            get { return _previousLine; }
            set
            {
                if (_previousLine.Equals(value)) return;
                _previousLine = value;
                RaisePropertyChanged("PreviousLine");
            }
        }

        #endregion

        #region NextLine (INotifyPropertyChanged Property)

        private TimeSpan _nextLine;

        public TimeSpan NextLine
        {
            get { return _nextLine; }
            set
            {
                if (_nextLine.Equals(value)) return;
                _nextLine = value;
                RaisePropertyChanged("NextLine");
            }
        }

        #endregion

        #region ScrollOffset DependencyProperty
        public static readonly DependencyProperty ScrollOffsetProperty =
            DependencyProperty.Register("ScrollOffset", typeof(double), typeof(DesktopLyric), new PropertyMetadata(default(double),OnScrollOffsetChanged));
        
        public double ScrollOffset
        {
            get { return (double)GetValue(ScrollOffsetProperty); }
            set { SetValue(ScrollOffsetProperty, value); }
        }

        private static void OnScrollOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var wd = d as DesktopLyric;
            if (wd == null) return;
            wd.LrcContaner.ScrollToVerticalOffset((double)e.NewValue);
        }
        #endregion

        public DesktopLyric(MainViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();

            IsOpened = true;
            ScrollSmooth = FindResource("ScroollSmooth") as Storyboard;
            ViewModel.MediaManager.PropertyChanged += MediaManagerPropertyChanged;
            Closed += (s, e) =>
            {
                IsOpened = false;
                ViewModel.MediaManager.PropertyChanged -= MediaManagerPropertyChanged;
            };
            Loaded += (s, e) => SetWindow();
            SourceInitialized += (s, e) =>
            {
                //Make window can be ignored by mouse event
                var hwnd = new WindowInteropHelper(this).Handle;
                var extendedStyle = NativeMethods.GetWindowLong(hwnd, (int)GWL.EXSTYLE);
                NativeMethods.SetWindowLong(hwnd, (int) GWL.EXSTYLE, extendedStyle | (int) (WSEX.TRANSPARENT) | (int) (WSEX.TOOLWINDOW));
            };
        }

        private void MediaManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Lyric"))
                SetWindow();
            else if (e.PropertyName.Equals("CurrnetLrcLine"))
            {
                var previous = ViewModel.MediaManager.CurrnetLrcLine.Key - 1;
                if (previous > 1)
                {
                    //LrcContaner.ScrollToVerticalOffset(41 * previous);
                    var anim = (DoubleAnimation)ScrollSmooth.Children[0];
                    //anim.From = 41*(previous - 1);
                    anim.To = 41 * previous;
                    ScrollSmooth.Begin();
                }

                var keys = ViewModel.MediaManager.LrcKeys;
                if (previous > -1) PreviousLine = keys[previous];
                var next = previous + 2;
                if (next < keys.Count) NextLine = keys[next];
            }
        }

        private void SetWindow() 
        {
            if (ViewModel.MediaManager.Lyric.Content.Count < 2)
                Hide();
            else
            {
                Width = 1000;
                Height = 120;
                Top = SystemParameters.WorkArea.Height - Height - 60;
                Left = (SystemParameters.WorkArea.Width - Width)/2;
                Show();
                LrcContaner.ScrollToTop();
            }
        }

    }
}
