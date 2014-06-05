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
using MusicFmApplication.ViewModel;
using Service.Model;

namespace MusicFmApplication.Controls
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

        public SongsViewer()
        {
            InitializeComponent();

        }
    }
}
