using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MusicFm.ViewModel;
using MusicFm.Model;

namespace MusicFm.Controls
{
    /// <summary>
    /// Interaction logic for SongsViewer.xaml
    /// </summary>
    public partial class SongsViewer : UserControl
    {
        #region DependencyProperty ViewModel
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(MainViewModel), typeof(SongsViewer), new PropertyMetadata(default(MainViewModel)));

        public MainViewModel ViewModel
        {
            get { return (MainViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        } 
        #endregion

        #region DependencyProperty SongList
        public static readonly DependencyProperty SongListProperty =
            DependencyProperty.Register("SongList", typeof(ObservableCollection<Song>), typeof(SongsViewer), new PropertyMetadata(default(ObservableCollection<Song>)));

        public ObservableCollection<Song> SongList
        {
            get { return (ObservableCollection<Song>)GetValue(SongListProperty); }
            set { SetValue(SongListProperty, value); }
        } 
        #endregion

        #region DependencyProperty IsBubblingScroll
        public static readonly DependencyProperty IsBubblingScrollProperty =
            DependencyProperty.Register("IsBubblingScroll", typeof(bool), typeof(SongsViewer), new PropertyMetadata(false));

        public bool IsBubblingScroll
        {
            get { return (bool)GetValue(IsBubblingScrollProperty); }
            set { SetValue(IsBubblingScrollProperty, value); }
        } 
        #endregion

        public SongsViewer()
        {
            InitializeComponent();
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!IsBubblingScroll || e.Handled) return;

            e.Handled = true;
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = MouseWheelEvent,
                Source = sender
            };
            var parent = ((Control)sender).Parent as UIElement;
            if (parent != null) parent.RaiseEvent(eventArg);
        }
    }
}
