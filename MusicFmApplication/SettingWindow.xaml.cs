using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CommonHelperLibrary.Dwm;
using MahApps.Metro.Controls;

namespace MusicFmApplication
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : MetroWindow
    {
        public MainViewModel ViewModel { get; set; }

        #region BackgroundColor(DependencyProperty)
        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.Register("BackgroundColor", typeof(SolidColorBrush), typeof(SettingWindow), new PropertyMetadata(default(SolidColorBrush)));

        public SolidColorBrush BackgroundColor
        {
            get { return (SolidColorBrush)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }
        #endregion

        public SettingWindow(MainViewModel viewModel)
        {
            ViewModel = viewModel;
            var rdm = new Random();
            BackgroundColor = new SolidColorBrush(Color.FromArgb(150, (byte)rdm.Next(0, 255), (byte)rdm.Next(0, 255), (byte)rdm.Next(0, 255)));

            InitializeComponent();

            Loaded += (s, e) => { viewModel.IsSettingWindowOpened = true; };
            Closed += (s, e) => { viewModel.IsSettingWindowOpened = false; };

            if (DwmHelper.IsDwmSupported)
            {
                DwmHelper = new DwmHelper(this);
                DwmHelper.AeroGlassEffectChanged += DwmHelperAeroGlassEffectChanged;
            }
        }

        protected DwmHelper DwmHelper;

        void DwmHelperAeroGlassEffectChanged(object sender, EventArgs e)
        {
            if (DwmHelper.IsAeroGlassEffectEnabled)
                DwmHelper.EnableBlurBehindWindow();
            else
                DwmHelper.EnableBlurBehindWindow(false);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            if (DwmHelper != null && DwmHelper.IsAeroGlassEffectEnabled)
            {
                DwmHelper.EnableBlurBehindWindow();
            }
        }
    }
}
