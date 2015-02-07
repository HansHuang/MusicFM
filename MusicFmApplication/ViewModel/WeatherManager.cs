using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using CommonHelperLibrary;
using CommonHelperLibrary.WEB;

namespace MusicFm.ViewModel
{
    public class WeatherManager : ViewModelBase
    {
        #region WeatherData (INotifyPropertyChanged Property)

        private Weather _weatherData;

        public Weather WeatherData
        {
            get { return _weatherData; }
            set
            {
                if (_weatherData != null && _weatherData.Equals(value)) return;
                _weatherData = value;
                RaisePropertyChanged("WeatherData");
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

        protected const string CacheName = "Weather";
        
        
        //protected readonly MainViewModel ViewModel;

        public WeatherManager(MainViewModel viewModel) 
        {
            ViewModel = viewModel;
            GetWeatherDetail();
        }

        private void GetWeatherDetail()
        {
            Task.Run(() =>
            {
                //Get Weather cache from file system
                var weatherInSetting = SettingHelper.GetSetting(CacheName, App.Name).Deserialize<Weather>();
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    WeatherData = weatherInSetting;
                });
                var weather = CityWeatherHelper.GetWeather();
                if (weather.LifeIndexes != null && weather.LifeIndexes.Count > 0 &&
                    !(weather.LifeIndexes is ObservableCollection<LifeIndex>))
                    weather.LifeIndexes = new ObservableCollection<LifeIndex>(weather.LifeIndexes);
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    WeatherData = weather;
                });
                SettingHelper.SetSetting(CacheName, weather.SerializeToString(), App.Name);
            });
        }
    }
}