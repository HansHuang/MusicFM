using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using MusicFmApplication.ViewModel;
using Service.Model;

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

        public ChannelsViewer()
        {
            InitializeComponent();
        }
    }
}
