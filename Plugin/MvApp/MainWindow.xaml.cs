using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

namespace MvPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(MainViewModel), typeof(MainWindow), new PropertyMetadata(default(MainViewModel)));

        public MainViewModel ViewModel
        {
            get { return (MainViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = MainViewModel.GetInstance(Dispatcher);

            Loaded += (s, e) =>
            {
                ViewModel.IsWindowLoaded = true;
                //ViewModel.MvList.CollectionChanged += MvListChanged;
            };
        }

        //private void MvListChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    if (e.Action == NotifyCollectionChangedAction.Add&&
        //        MainGrid.Children.Count==0)
        //    {
        //        var binding = new Binding("ViewModel.PlayPara");
        //        var player = new FlvPlayer.FlvPlayer();
        //        player.SetBinding(player.PlayerParameter, binding);

        //        MainGrid.Children.Add(player);
        //    }
        //}
    }
}
