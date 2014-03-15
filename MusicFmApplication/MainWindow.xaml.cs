using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using CommonHelperLibrary.Dwm;
using MahApps.Metro.Controls;

namespace MusicFmApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [TemplatePart(Name = PART_TitleBarBackground, Type = typeof(Rectangle))]
    public partial class MainWindow : MetroWindow
    {
        private const string PART_TitleBarBackground = "PART_WindowTitleBackground";

        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(MainViewModel), typeof(MainWindow), new PropertyMetadata(default(MainViewModel),OnViewModelChanged));

        public MainViewModel ViewModel
        {
            get { return (MainViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var wd = d as MainWindow;
            var viewModel = e.NewValue as MainViewModel;
            if (wd == null || viewModel == null) return;
            viewModel.MediaManager.PropertyChanged += (s, arg) =>
            {
                if (arg.PropertyName.Equals("SongPictureColor"))
                {
                    var storyboard = wd.FindResource("BackgroundColorStoryboard") as Storyboard;
                    if (storyboard == null) return;
                    var color = viewModel.MediaManager.SongPictureColor;
                    color.A = 150;
                    //Make color to darker
                    const double offset = .4;
                    color.R = (byte)(color.R * offset);
                    color.G = (byte)(color.G * offset);
                    color.B = (byte)(color.B * offset);
                    ((ColorAnimation) storyboard.Children[0]).To = color;
                    storyboard.Begin();
                }
                else if (arg.PropertyName.Equals("SongPicture"))
                {
                    var storyboard = wd.FindResource("AlbumPictureFadeInStoryboard") as Storyboard;
                    if (storyboard == null) return;
                    storyboard.Begin();
                }
            };
        }

        #endregion

        #region BackgroundColor(DependencyProperty)
        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.Register("BackgroundColor", typeof(Color), typeof(MainWindow), new PropertyMetadata(default(Color),OnBackgroundColorChanged));

        private static void OnBackgroundColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) 
        {
            var wnd = d as MainWindow;
            if (wnd == null || !(e.NewValue is Color)) return;
            var color = (Color) e.NewValue;
            wnd.BackgroundColorBrush = new SolidColorBrush(color);
        }

        public Color BackgroundColor
        {
            get { return (Color)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }
        #endregion

        #region BackgroundColorBrush DependencyProperty
        public static readonly DependencyProperty BackgroundColorBrushProperty =
            DependencyProperty.Register("BackgroundColorBrush", typeof(Brush), typeof(MainWindow), new PropertyMetadata(default(Brush)));

        public Brush BackgroundColorBrush
        {
            get { return (Brush)GetValue(BackgroundColorBrushProperty); }
            set { SetValue(BackgroundColorBrushProperty, value); }
        } 
        #endregion

        public MainWindow()
        {
            //var rdm = new Random();
            //BackgroundColor = new SolidColorBrush(Color.FromArgb(150, (byte)rdm.Next(0, 255), (byte)rdm.Next(0, 255), (byte)rdm.Next(0, 255)));

            InitializeComponent();

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
            {
                DwmHelper.EnableBlurBehindWindow();
            }
            else
            {
                DwmHelper.EnableBlurBehindWindow(false);
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            if (DwmHelper != null && DwmHelper.IsAeroGlassEffectEnabled)
            {
                DwmHelper.EnableBlurBehindWindow();
            }

            Task.Run(() =>
                {
                    Thread.Sleep(500);
                    Dispatcher.InvokeAsync(() =>
                        {
                            ViewModel = MainViewModel.GetInstance(this);
                        });
                });
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var titleBarBg = GetTemplateChild(PART_TitleBarBackground) as Rectangle;
            if (titleBarBg != null) titleBarBg.Opacity = .02;
        }

        private void MainGridOnPreviewMouseDown(object sender, MouseButtonEventArgs e) {
            this.DragMove();
        }

    }
}
