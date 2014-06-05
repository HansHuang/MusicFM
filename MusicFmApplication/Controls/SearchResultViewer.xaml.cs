using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CustomControlResources;
using MusicFmApplication.ViewModel;
using Service.Model;

namespace MusicFmApplication.Controls
{
    /// <summary>
    /// Interaction logic for SearchResultViewer.xaml
    /// </summary>
    public partial class SearchResultViewer : UserControl,INotifyPropertyChanged
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

        #region DependencyProperty ViewModel
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(MainViewModel), typeof(SearchResultViewer), new PropertyMetadata(default(MainViewModel)));

        public MainViewModel ViewModel
        {
            get { return (MainViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        } 
        #endregion

        #region DependencyProperty SearchResult
        public static readonly DependencyProperty SearchResultProperty =
            DependencyProperty.Register("SearchResult", typeof(SearchResult), typeof(SearchResultViewer), new PropertyMetadata(default(SearchResult)));

        public SearchResult SearchResult
        {
            get { return (SearchResult)GetValue(SearchResultProperty); }
            set { SetValue(SearchResultProperty, value); }
        } 
        #endregion

        #region RelayCommand HideResultViewerCmd

        private RelayCommand _hideResultViewerCmd;

        public ICommand HideResultViewerCmd
        {
            get { return _hideResultViewerCmd ?? (_hideResultViewerCmd = new RelayCommand(s => HideResultViewer())); }
        }

        private void HideResultViewer()
        {
            ViewModel.SearchResult = null;
        }

        #endregion

        public SearchResultViewer()
        {
            InitializeComponent();
        }
    }
}
