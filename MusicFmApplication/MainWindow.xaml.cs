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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using CustomControlResources.Aero;
using CustomControlResources.Interop;
using MahApps.Metro.Controls;

namespace MusicFmApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow: MetroWindow
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
            ViewModel = MainViewModel.GetInstance(this);

            //Enable Aero Grass Effect
            SourceInitialized += (s, e) =>
                {
                    if (Environment.OSVersion.Version.Major < 6) return;

                    AeroGlassCompositionChanged += (sender, args) =>
                        {
                            if (args.GlassAvailable)
                                AeroHelper.EnableBlurBehindWindow(this);
                        };

                    //Add hock to get DWM info
                    var interopHelper = new WindowInteropHelper(this);
                    var source = HwndSource.FromHwnd(interopHelper.Handle);
                    if (source == null) return;
                    source.AddHook(WndProc);

                    //Try to enable aero effect
                    AeroHelper.EnableBlurBehindWindow(this);
                };

            //SizeChanged += (sender, args) =>
            //    {
            //        if (Environment.OSVersion.Version.Major < 6) return;
            //        AeroHelper.EnableBlurBehindWindow(this);
            //    };
        }

        #region Aero Glass Effect
        public event EventHandler<AeroGlassCompositionChangedEventArgs> AeroGlassCompositionChanged;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM.DwmCompositionChanged || msg == WM.DwmnCrenderingChanged)
            {
                if (AeroGlassCompositionChanged != null)
                {
                    AeroGlassCompositionChanged(this,
                        new AeroGlassCompositionChangedEventArgs(AeroHelper.AeroGlassCompositionEnabled));
                }

                handled = true;
            }
            return IntPtr.Zero;
        } 
        #endregion

    }
}
