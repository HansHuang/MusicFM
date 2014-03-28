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
using System.Windows.Shapes;

namespace MusicFmApplication
{
    /// <summary>
    /// Interaction logic for DesktopLyric.xaml
    /// </summary>
    public partial class DesktopLyric : Window
    {
        public static bool IsOpened = false;

        public MainViewModel ViewModel { get; set; }

        public DesktopLyric(MainViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();

            IsOpened = true;
            Closed += (s, e) => { IsOpened = false; };
        }
    }
}
