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

        private const double WindowOpacity = .625;

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
                    //Make color to darker
                    const double offset = .4;
                    color.R = (byte)(color.R * offset);
                    color.G = (byte)(color.G * offset);
                    color.B = (byte)(color.B * offset);
                    color.A = (byte)(WindowOpacity*256);
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
            DependencyProperty.Register("BackgroundColor", typeof(Color), typeof(MainWindow), new PropertyMetadata(default(Color)));

        public Color BackgroundColor
        {
            get { return (Color)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            if (DwmHelper.IsDwmSupported)
            {
                DwmHelper = new DwmHelper(this);
                DwmHelper.AeroGlassEffectChanged += DwmHelperAeroGlassEffectChanged;
            }

            SizeChanged += (s, e) =>
            {
                if (DwmHelper != null && DwmHelper.IsAeroGlassEffectEnabled)
                    DwmHelper.EnableBlurBehindWindow();
            };

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

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState.Equals(WindowState.Minimized))
            {
                //var storyboard = FindResource("WindowFadeoutStoryboard") as Storyboard;
                //if (storyboard != null) {
                //    WindowState = WindowState.Normal;
                //    storyboard.Begin();
                //}
                var miniWnd = new MiniWindow(ViewModel)
                {
                    Left = Left + 80,
                    Top = Top + 100
                };
                miniWnd.Show();
            }
            base.OnStateChanged(e);
            Visibility = Visibility.Collapsed;
        }
        

    }
}
