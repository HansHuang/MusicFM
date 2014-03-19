using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using CommonHelperLibrary.Dwm;
using MahApps.Metro.Controls;
using Forms=System.Windows.Forms;
using Color = System.Windows.Media.Color;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace MusicFmApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [TemplatePart(Name = PartTitleBarBackground, Type = typeof(Rectangle))]
    public partial class MainWindow : MetroWindow
    {
        private const string PartTitleBarBackground = "PART_WindowTitleBackground";

        private const double WindowOpacity = .625;

        private MiniWindow _miniWindow;

        private bool _isQuitApp;
        private bool _isNeedShowMiniWindow = true;
        private bool _hasNotifyIconTip;


        public Forms.NotifyIcon NotifyIcon;

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

            SetNotifyIcon();

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

            StateChanged += MainWindowStateChanged;
        }

        private void SetNotifyIcon()
        {
            Action hideWindow = () =>
            {
                _isNeedShowMiniWindow = false;
                ShowInTaskbar = false;
                WindowState = WindowState.Minimized;
                _isNeedShowMiniWindow = true;

                if (_hasNotifyIconTip) return;
                NotifyIcon.ShowBalloonTip(1000, "I'm here", "Click me to show again", Forms.ToolTipIcon.Info);
                _hasNotifyIconTip = true;
            };
            Action showWindow = () =>
            {
                if (_miniWindow != null && _miniWindow.Visibility == Visibility.Visible)
                    _miniWindow.Hide();
                WindowState = WindowState.Normal;
                ShowInTaskbar = true;
            };
            Action closeWindow = () => 
            {
                _isQuitApp = true;
                NotifyIcon.Visible = false;
                NotifyIcon.Dispose();
                if(_miniWindow!=null) _miniWindow.Close();
                Close();
            };

            if (NotifyIcon == null)
            {
                var iconUri = new Uri("/MusicFmApplication;component/logo.ico", UriKind.RelativeOrAbsolute);
                var iconStream = Application.GetResourceStream(iconUri);
                if (iconStream != null)
                {
                    NotifyIcon = new Forms.NotifyIcon
                    {
                        Icon = new Icon(iconStream.Stream),
                        Text = LocalTextHelper.GetLocText("MusicFm"),
                        Visible = true,
                        ContextMenuStrip = GetNotifyIconMenuStrip(showWindow, closeWindow),
                    };
                    NotifyIcon.MouseClick += (s, a) =>
                    {
                        if (!a.Button.Equals(Forms.MouseButtons.Left)) return;
                        if (WindowState.Equals(WindowState.Minimized)) showWindow();
                        else hideWindow();
                    };
                }
            }

            Closing += (s, e) =>
            {
                if (_isQuitApp) return;
                e.Cancel = true;
                hideWindow();
            };
        }

        private Forms.ContextMenuStrip GetNotifyIconMenuStrip(Action showWindow, Action closeWindow) 
        {
            var ctxMenu = new Forms.ContextMenuStrip();

            var openUri = new Uri("/CustomControlResources;component/Images/183.24.png", UriKind.RelativeOrAbsolute);
            var openStream = Application.GetResourceStream(openUri);
            var openIcon = Image.FromStream(openStream == null ? new MemoryStream() : openStream.Stream);
            var openMainMi = new Forms.ToolStripMenuItem { Text = LocalTextHelper.GetLocText("OpenMainWd"), Image = openIcon };
            openMainMi.Click += (s, e) => showWindow();

            var miniUri = new Uri("/CustomControlResources;component/Images/103.24.png", UriKind.RelativeOrAbsolute);
            var miniStream = Application.GetResourceStream(miniUri);
            var miniIcon = Image.FromStream(miniStream == null ? new MemoryStream() : miniStream.Stream);
            var showMiniItem = new Forms.ToolStripMenuItem { Text = LocalTextHelper.GetLocText("OpenMiniWd"), Image = miniIcon };
            showMiniItem.Click += (s, e) =>
            {
                WindowState = WindowState.Minimized;
                MainWindowStateChanged(null, null);
            };

            var quitUri = new Uri("/CustomControlResources;component/Images/288.24.png", UriKind.RelativeOrAbsolute);
            var quitStream = Application.GetResourceStream(quitUri);
            var quitIcon = Image.FromStream(quitStream == null ? new MemoryStream() : quitStream.Stream);
            var closeWndMi = new Forms.ToolStripMenuItem { Text = LocalTextHelper.GetLocText("QuitApp"), Image = quitIcon };
            closeWndMi.Click += (s, e) => closeWindow();

            ctxMenu.Items.Add(openMainMi);
            ctxMenu.Items.Add(new Forms.ToolStripSeparator());
            ctxMenu.Items.Add(showMiniItem);
            ctxMenu.Items.Add(new Forms.ToolStripSeparator());
            ctxMenu.Items.Add(closeWndMi);
            return ctxMenu;
        }

        private void MainWindowStateChanged(object sender, EventArgs e)
        {
            if (!WindowState.Equals(WindowState.Minimized) || !_isNeedShowMiniWindow) return;
            //var storyboard = FindResource("WindowFadeoutStoryboard") as Storyboard;
            //if (storyboard != null) {
            //    WindowState = WindowState.Normal;
            //    storyboard.Begin();
            //}
            ShowInTaskbar = false;
            if (_miniWindow == null)
            {
                _miniWindow = new MiniWindow(ViewModel);
                _miniWindow.Closing += (s, a) => 
                {
                    if (_isQuitApp) return;
                    a.Cancel = true;
                    _miniWindow.Hide();
                    if (_miniWindow.IsMinimizeToIcon)
                    {
                        Close();
                        return;
                    }
                    Left = _miniWindow.Left - 80;
                    if (Left < 0) Left = 0;
                    Top = _miniWindow.Top - 100;
                    if (Top < 0) Top = 0;
                    ShowInTaskbar = true;
                    WindowState = WindowState.Normal;
                    //var storyboard = wd.FindResource("WindowFadeInStoryboard") as Storyboard;
                    //if (storyboard != null) storyboard.Begin();
                };
            }
            if (sender != null)
            {
                _miniWindow.Left = Left + 80;
                _miniWindow.Top = Top + 100;
            }
            
            _miniWindow.Show();
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

            var titleBarBg = GetTemplateChild(PartTitleBarBackground) as Rectangle;
            if (titleBarBg != null) titleBarBg.Opacity = .02;
        }
        

    }
}
