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
using MusicFmApplication.ViewModel;

namespace MusicFmApplication
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : MetroWindow 
    {
        public static bool IsOpened = false;

        protected MainViewModel ViewModel { get; set; }

        #region BackgroundColor(DependencyProperty)
        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.Register("BackgroundColor", typeof(Color), typeof(SettingWindow), new PropertyMetadata(default(Color)));

        public Color BackgroundColor
        {
            get { return (Color)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }
        #endregion

        public SettingWindow(MainViewModel viewModel)
        {
            ViewModel = viewModel;
            var rdm = new Random();
            BackgroundColor = Color.FromArgb(215, (byte)rdm.Next(0, 150), (byte)rdm.Next(0, 150), (byte)rdm.Next(0, 150));

            InitializeComponent();

            IsOpened = true;
            Closed += (s, e) => { IsOpened = false; };

            if (DwmHelper.IsDwmSupported)
            {
                DwmHelper = new DwmHelper(this);
                DwmHelper.AeroGlassEffectChanged += DwmHelperAeroGlassEffectChanged;
            }
        }

        #region Aero Glass Effect
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
        #endregion
    }
}
