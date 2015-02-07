using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using CommonHelperLibrary.Dwm;
using MahApps.Metro.Controls;
using MusicFm.ViewModel;

namespace MusicFm
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : MetroWindow,INotifyPropertyChanged
    {
        #region INotifyPropertyChanged RaisePropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        public static bool IsOpened = false;

        #region BackgroundColor (INotifyPropertyChanged Property)

        private SolidColorBrush _backgroundColor;

        public SolidColorBrush BackgroundColor
        {
            get { return _backgroundColor; }
            set
            {
                if (_backgroundColor != null && _backgroundColor.Equals(value)) return;
                _backgroundColor = value;
                RaisePropertyChanged("BackgroundColor");
            }
        }

        #endregion

        #region ViewModel (INotifyPropertyChanged Property)

        private MainViewModel _viewModel;

        public MainViewModel ViewModel
        {
            get { return _viewModel; }
            set
            {
                if (_viewModel != null && _viewModel.Equals(value)) return;
                _viewModel = value;
                RaisePropertyChanged("ViewModel");
            }
        }

        #endregion

        #region AboutTxt (INotifyPropertyChanged Property)

        private string _aboutTxt;

        public string AboutTxt
        {
            get { return _aboutTxt; }
            set
            {
                if (_aboutTxt != null && _aboutTxt.Equals(value)) return;
                _aboutTxt = value;
                RaisePropertyChanged("AboutTxt");
            }
        }

        #endregion

        public AboutWindow(MainViewModel viewModel) 
        {
            ViewModel = viewModel;
            var color = viewModel.MediaManager.SongPictureColor;
            color.A = 200;
            BackgroundColor = new SolidColorBrush(color);
            AboutTxt = GetAboutTxt();
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

        private string GetAboutTxt() {
            return "    Hans Huang @ Jungo Studio";
        }
    }
}
