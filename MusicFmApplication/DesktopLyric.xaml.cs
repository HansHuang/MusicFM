using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

        public DesktopLyric(MainViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();

            IsOpened = true;
            ViewModel.MediaManager.PropertyChanged += MediaManagerPropertyChanged;
            Closed += (s, e) =>
            {
                IsOpened = false;
                ViewModel.MediaManager.PropertyChanged -= MediaManagerPropertyChanged;
            };
            Loaded += (s, e) => SetWindow();
        }

        private void MediaManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Lyric"))
                SetWindow();
            else if (e.PropertyName.Equals("CurrnetLrcLine"))
            {
                var previous = ViewModel.MediaManager.CurrnetLrcLine.Key - 1;
                if (previous > 1)
                    LrcContaner.ScrollToVerticalOffset(41 * previous);

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
