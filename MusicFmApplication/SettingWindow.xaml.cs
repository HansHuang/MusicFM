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
using MusicFm.ViewModel;

namespace MusicFm
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    [TemplatePart(Name = PartTitleBarBackground, Type = typeof(Rectangle))]
    [TemplatePart(Name = TitleTextbolck, Type = typeof(TextBlock))]
    public partial class SettingWindow : MetroWindow 
    {
        private const string PartTitleBarBackground = "PART_WindowTitleBackground";
        private const string TitleTextbolck = "WindowTitleTextBlock";

        public static bool IsOpened = false;

        public MainViewModel ViewModel { get; set; }

        protected static double WindowOpacity = .68;

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
            InitializeComponent();
            IsOpened = true;
            Closed += (s, e) => { IsOpened = false; };

            if (DwmHelper.IsDwmSupported)
            {
                DwmHelper = new DwmHelper(this);
                DwmHelper.AeroGlassEffectChanged += DwmHelperAeroGlassEffectChanged;
            }
            else WindowOpacity = .9;

            var color = viewModel.MediaManager.SongPictureColor;
            color.A = (byte)(WindowOpacity * 256);
            BackgroundColor = color;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var titleBarBg = GetTemplateChild(PartTitleBarBackground) as Rectangle;
            if (titleBarBg != null) titleBarBg.Opacity = .02;

            var titleTb = GetTemplateChild(TitleTextbolck) as TextBlock;
            if (titleTb != null) titleTb.FontWeight = FontWeights.ExtraBold;
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
