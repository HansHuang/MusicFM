using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MusicFmApplication.ViewModel;
using MusicFmApplication.Model;

namespace MusicFmApplication.Controls
{
    /// <summary>
    /// Interaction logic for ChannelsViewer.xaml
    /// </summary>
    public partial class ChannelsViewer : UserControl
    {
        #region ViewModel DependencyProperty
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(MainViewModel), typeof(ChannelsViewer), new PropertyMetadata(default(MainViewModel)));

        public MainViewModel ViewModel
        {
            get { return (MainViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        } 
        #endregion

        #region Channels DependencyProperty
        public static readonly DependencyProperty ChannelsProperty =
            DependencyProperty.Register("Channels", typeof(ObservableCollection<Channel>), typeof(ChannelsViewer), new PropertyMetadata(default(ObservableCollection<Channel>)));

        public ObservableCollection<Channel> Channels
        {
            get { return (ObservableCollection<Channel>)GetValue(ChannelsProperty); }
            set { SetValue(ChannelsProperty, value); }
        }
        #endregion

        #region DependencyProperty IsBubblingScroll
        public static readonly DependencyProperty IsBubblingScrollProperty =
            DependencyProperty.Register("IsBubblingScroll", typeof(bool), typeof(ChannelsViewer), new PropertyMetadata(false));

        public bool IsBubblingScroll
        {
            get { return (bool)GetValue(IsBubblingScrollProperty); }
            set { SetValue(IsBubblingScrollProperty, value); }
        }
        #endregion

        public ChannelsViewer()
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
