using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using CommonHelperLibrary.Dwm;
using MahApps.Metro.Controls;
using MusicFm.Helper;
using MusicFm.Helper;
using MusicFm.ViewModel;
using Forms=System.Windows.Forms;
using Color = System.Windows.Media.Color;
using Image = System.Drawing.Image;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace MusicFm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [TemplatePart(Name = PartTitleBarBackground, Type = typeof(Rectangle))]
    [TemplatePart(Name = TitleTextbolck, Type = typeof(TextBlock))]
    public partial class MainWindow : MetroWindow
    {
        #region Fields
        private const string PartTitleBarBackground = "PART_WindowTitleBackground";
        private const string TitleTextbolck = "WindowTitleTextBlock";
        private const double WindowOpacity = .68;

        private MiniWindow _miniWindow;

        private bool _isQuitApp;
        private bool _hasNotifyIconTip;

        public Forms.NotifyIcon NotifyIcon; 
        #endregion

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
                    var color = viewModel.MediaManager.SongPictureColor;
                    //Make color to lighter
                    const double offset = 1.2;
                    color.R = (byte)(color.R * offset);
                    color.G = (byte)(color.G * offset);
                    color.B = (byte)(color.B * offset);
                    color.A = (byte)(WindowOpacity * 256);
                    if (wd.IsLoaded)
                    {
                        var storyboard = wd.FindResource("BackgroundColorStoryboard") as Storyboard;
                        if (storyboard == null) return;
                        ((ColorAnimation)storyboard.Children[0]).To = color;
                        storyboard.Begin();
                    }
                    else wd.BackgroundColor = color;
                }
                else if (arg.PropertyName.Equals("SongPicture"))
                {
                    var storyboard = wd.FindResource("AlbumPictureFadeInStoryboard") as Storyboard;
                    if (storyboard == null) return;
                    storyboard.Begin();
                }
                else if (arg.PropertyName.Equals("Lyric"))
                {
                    wd.LrcContaner.ScrollToTop();
                }
                else if (arg.PropertyName.Equals("CurrnetLrcLine"))
                {
                    var lineIndex = viewModel.MediaManager.CurrnetLrcLine.Key;
                    //not scroll at first 5 lines
                    if (lineIndex < 5) return;
                    wd.LrcContaner.LineDown();
                    //Scroll two lines every 3 times
                    if (lineIndex % 3 == 1)
                        wd.LrcContaner.LineDown();
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
            //set the initial color
            var rdm = new Random();
            BackgroundColor = Color.FromArgb(160, (byte)rdm.Next(150, 255), (byte)rdm.Next(150, 255), (byte)rdm.Next(150, 255));

            if (DwmHelper.IsDwmSupported)
            {
                DwmHelper = new DwmHelper(this);
                DwmHelper.AeroGlassEffectChanged += DwmHelperAeroGlassEffectChanged;
            }

            SetNotifyIcon();

            //SizeChanged += (s, e) =>
            //{
            //    if (DwmHelper != null && DwmHelper.IsAeroGlassEffectEnabled)
            //        DwmHelper.EnableBlurBehindWindow();
            //};

            StateChanged += MainWindowStateChanged;

            Loaded += delegate { ViewModel = MainViewModel.GetInstance(this); };

        }

        #region Set Aero Glass Effect
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
                DwmHelper.EnableBlurBehindWindow();
        }

        #endregion

        #region Processor for NotifyIcon
        private void SetNotifyIcon()
        {
            Action hideWindow = () =>
            {
                Hide();
                if (_hasNotifyIconTip) return;
                NotifyIcon.ShowBalloonTip(1000, "I'm here", "Click me to show again", Forms.ToolTipIcon.Info);
                _hasNotifyIconTip = true;
            };
            Action showWindow = () =>
            {
                if (_miniWindow != null) _miniWindow.Close();
                Show();
            };
            Action quitApp = () =>
            {
                _isQuitApp = true;
                NotifyIcon.Visible = false;
                NotifyIcon.Dispose();
                if (_miniWindow != null) _miniWindow.Close();
                Close();
            };

            if (NotifyIcon == null)
            {
                var iconUri = new Uri("/MusicFm;component/logo.ico", UriKind.RelativeOrAbsolute);
                var iconStream = Application.GetResourceStream(iconUri);
                if (iconStream != null)
                {
                    NotifyIcon = new Forms.NotifyIcon
                    {
                        Icon = new Icon(iconStream.Stream),
                        Text = LocalTextHelper.GetLocText("MusicFm"),
                        Visible = true,
                        ContextMenuStrip = GetNotifyIconMenuStrip(showWindow, quitApp),
                    };
                    NotifyIcon.MouseClick += (s, a) =>
                    {
                        if (!a.Button.Equals(Forms.MouseButtons.Left)) return;
                        if (_miniWindow != null)
                            if (_miniWindow.Visibility == Visibility.Visible) _miniWindow.Hide();
                            else _miniWindow.Show();
                        else if (Visibility == Visibility.Visible)
                            hideWindow();
                        else
                            showWindow();
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
            Func<string, Image> getImage = s =>
            {
                var uri = new Uri(s, UriKind.RelativeOrAbsolute);
                var stream = Application.GetResourceStream(uri);
                return Image.FromStream(stream == null ? new MemoryStream() : stream.Stream);
            };
            var ctxMenu = new Forms.ContextMenuStrip();

            var playIcon = getImage("/CustomControlResources;component/Images/play32.png");
            var pauseIcon = getImage("/CustomControlResources;component/Images/pause32.png");
            var playMi = new Forms.ToolStripMenuItem(LocalTextHelper.GetLocText("Play"), playIcon);
            playMi.Click += (s, e) => ViewModel.MediaManager.PauseResumePlayerCmd.Execute(null);


            var likeIcon = getImage("/CustomControlResources;component/Images/heart32.png");
            var likedIcon = getImage("/CustomControlResources;component/Images/RedHeart24.png");
            var likeMi = new Forms.ToolStripMenuItem(LocalTextHelper.GetLocText("LikeSong"), likeIcon);
            likeMi.Click += (s, e) => ViewModel.LikeSongCmd.Execute("");

            var deleteIcon = getImage("/CustomControlResources;component/Images/delete32.png");
            var deleteMi = new Forms.ToolStripMenuItem(LocalTextHelper.GetLocText("HateSong"), deleteIcon);
            deleteMi.Click += (s, e) => ViewModel.LikeSongCmd.Execute("1");

            var nextIcon = getImage("/CustomControlResources;component/Images/next32.png");
            var nextMi = new Forms.ToolStripMenuItem(LocalTextHelper.GetLocText("NextSong"), nextIcon);
            nextMi.Click += (s, e) => ViewModel.NextSongCmd.Execute(false);

            var downloadIcon = getImage("/CustomControlResources;component/Images/download32.png");
            var downloadMi = new Forms.ToolStripMenuItem(LocalTextHelper.GetLocText("DownloadSong"), downloadIcon);
            downloadMi.Click += (s, e) => ViewModel.DownloadSongCmd.Execute(null);

            var folderIcon = getImage("/CustomControlResources;component/Images/folder32.png");
            var folderMi = new Forms.ToolStripMenuItem(LocalTextHelper.GetLocText("OpenDownloadFolder"), folderIcon) { Visible = false };
            folderMi.Click += (s, e) => ViewModel.OpenDownloadFolderCmd.Execute(null);

            var openIcon = getImage("/CustomControlResources;component/Images/183.24.png");
            var openMainMi = new Forms.ToolStripMenuItem { Text = LocalTextHelper.GetLocText("OpenMainWd"), Image = openIcon };
            openMainMi.Click += (s, e) => showWindow();

            var miniIcon = getImage("/CustomControlResources;component/Images/103.24.png");
            var showMiniItem = new Forms.ToolStripMenuItem { Text = LocalTextHelper.GetLocText("OpenMiniWd"), Image = miniIcon };
            showMiniItem.Click += (s, e) => 
            {
                if (_miniWindow == null) WindowState = WindowState.Minimized;
                else _miniWindow.Show();
            };

            var lrcIcon = getImage("/CustomControlResources;component/Images/220.32.png");
            var lrcMi = new Forms.ToolStripMenuItem(LocalTextHelper.GetLocText("OpenDeskLrc"), lrcIcon);
            lrcMi.Click += (s, e) =>
            {
                ViewModel.ToggleDesktopLyricCmd.Execute(null);
                var open = LocalTextHelper.GetLocText("OpenDeskLrc");
                var shut = LocalTextHelper.GetLocText("ShutDeskLrc");
                lrcMi.Text = lrcMi.Text == open ? shut : open;
            };

            var quitIcon = getImage("/CustomControlResources;component/Images/288.24.png");
            var closeWndMi = new Forms.ToolStripMenuItem { Text = LocalTextHelper.GetLocText("QuitApp"), Image = quitIcon };
            closeWndMi.Click += (s, e) => closeWindow();

            ctxMenu.Items.Add(playMi);
            ctxMenu.Items.Add(new Forms.ToolStripSeparator());
            ctxMenu.Items.Add(likeMi);
            ctxMenu.Items.Add(deleteMi);
            ctxMenu.Items.Add(nextMi);
            ctxMenu.Items.Add(new Forms.ToolStripSeparator());
            ctxMenu.Items.Add(downloadMi);
            ctxMenu.Items.Add(folderMi);
            ctxMenu.Items.Add(new Forms.ToolStripSeparator());
            ctxMenu.Items.Add(openMainMi);
            ctxMenu.Items.Add(showMiniItem);
            ctxMenu.Items.Add(lrcMi);
            ctxMenu.Items.Add(new Forms.ToolStripSeparator());
            ctxMenu.Items.Add(closeWndMi);

            ctxMenu.Opening += (s, e) => 
            {
                playMi.Image = ViewModel.MediaManager.IsPlaying ? pauseIcon : playIcon;
                playMi.Text = LocalTextHelper.GetLocText(ViewModel.MediaManager.IsPlaying ? "Pause" : "Play");

                if (ViewModel.CurrentSong != null && ViewModel.CurrentSong.Like == 1)
                    likeMi.Image = likedIcon;
                else
                    likeMi.Image = likeIcon;

                folderMi.Visible = ViewModel.DownloadProgress == 100;
                downloadMi.Visible = ViewModel.DownloadProgress == 0;

                openMainMi.Enabled = Visibility == Visibility.Hidden;
                showMiniItem.Enabled = _miniWindow == null || _miniWindow.Visibility == Visibility.Hidden;
            };
            return ctxMenu;
        } 
        #endregion

        #region Set the title bar to transparent and the sytle of window title
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var titleBarBg = GetTemplateChild(PartTitleBarBackground) as Rectangle;
            if (titleBarBg != null) titleBarBg.Opacity = .02;

            var titleTb = GetTemplateChild(TitleTextbolck) as TextBlock;
            if (titleTb != null) titleTb.FontWeight = FontWeights.ExtraBold;
        } 
        #endregion
        
        private void MainWindowStateChanged(object sender, EventArgs e) 
        {
            switch (WindowState)
            {
                case WindowState.Maximized://Prevent maximizing window
                    WindowState = WindowState.Normal;
                    return;
                case WindowState.Normal:
                    return;
                case WindowState.Minimized:
                    WindowState = WindowState.Normal;
                    break;
            }
            //var storyboard = FindResource("WindowFadeoutStoryboard") as Storyboard;
            //if (storyboard != null) storyboard.Begin();
            Hide();
            if (_miniWindow == null) 
            {
                _miniWindow = new MiniWindow(ViewModel) {Owner = this};
                _miniWindow.Closing += (s, a) => 
                {
                    if (_isQuitApp) return;
                    if (_miniWindow.IsMinimizeToIcon)
                    {
                        Close();
                        return;
                    }
                    Left = _miniWindow.Left - 80;
                    if (Left < 0) Left = 0;
                    Top = _miniWindow.Top - 100;
                    if (Top < 0) Top = 0;
                    Show();
                    //var fadeIn = FindResource("WindowFadeInStoryboard") as Storyboard;
                    //if (fadeIn != null) fadeIn.Begin();
                };
                _miniWindow.Closed += (s, a) => { _miniWindow = null; };
            }
            _miniWindow.Left = Left + 80;
            _miniWindow.Top = Top + 100;
            _miniWindow.Topmost = true;
            _miniWindow.ShowInTaskbar = false;
            _miniWindow.Show();
        }

    }
}
