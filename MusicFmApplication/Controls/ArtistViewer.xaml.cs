using System;
using System.Collections.Generic;
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
using Service.Model;

namespace MusicFm.Controls
{
    /// <summary>
    /// Interaction logic for ArtistViewer.xaml
    /// </summary>
    public partial class ArtistViewer : UserControl
    {

        #region DependencyProperty ViewModel
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(MainViewModel), typeof(ArtistViewer), new PropertyMetadata(default(MainViewModel)));

        public MainViewModel ViewModel
        {
            get { return (MainViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        #endregion

        #region DependencyProperty Artist
        public static readonly DependencyProperty ArtistProperty =
            DependencyProperty.Register("Artist", typeof(Artist), typeof(ArtistViewer), new PropertyMetadata(default(Artist)));

        public Artist Artist
        {
            get { return (Artist)GetValue(ArtistProperty); }
            set { SetValue(ArtistProperty, value); }
        } 
        #endregion

        public ArtistViewer()
        {
            InitializeComponent();
        }
    }
}
