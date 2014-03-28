using System;
using System.Windows;
using System.Windows.Input;
using CustomControlResources;

namespace MusicFmApplication
{
    /// <summary>
    /// Interaction logic for MiniWindow.xaml
    /// </summary>
    public partial class MiniWindow : Window
    {
        public bool IsMinimizeToIcon { get; private set; }

        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(MainViewModel), typeof(MiniWindow), new PropertyMetadata(default(MainViewModel)));

        public MainViewModel ViewModel
        {
            get { return (MainViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        #endregion

        #region RelayCommand SetTopMostCmd

        private RelayCommand _setTopMostCmd;

        public ICommand SetTopMostCmd
        {
            get { return _setTopMostCmd ?? (_setTopMostCmd = new RelayCommand(s => SetTopMostExecute())); }
        }

        private void SetTopMostExecute()
        {
            Topmost = !Topmost;
            ShowInTaskbar = !Topmost;
        }

        #endregion

        #region RelayCommand BackMainWindowCmd

        private RelayCommand _backMainWindowCmd;

        public ICommand BackMainWindowCmd
        {
            get { return _backMainWindowCmd ?? (_backMainWindowCmd = new RelayCommand(param => BackMainWindow())); }
        }

        private void BackMainWindow() 
        {
            IsMinimizeToIcon = false;
            Topmost = false;
            Close();
        }

        #endregion

        #region RelayCommand MinimizeToIconCmd

        private RelayCommand _minimizeToIconCmd;

        public ICommand MinimizeToIconCmd
        {
            get { return _minimizeToIconCmd ?? (_minimizeToIconCmd = new RelayCommand(param => MinimizeToIcon())); }
        }

        private void MinimizeToIcon()
        {
            IsMinimizeToIcon = true;
            Close();
        }

        #endregion

        public MiniWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            StateChanged += WindowStateChanged;
        }

        private void WindowStateChanged(object sender, EventArgs e) 
        {
            //Prevent maximizing window
            if (WindowState == WindowState.Maximized) WindowState = WindowState.Normal;
        }

        private void MainGridOnLeftMouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }


    }
}