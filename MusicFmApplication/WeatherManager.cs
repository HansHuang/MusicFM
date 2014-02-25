using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonHelperLibrary;
using CommonHelperLibrary.WEB;
using CustomControlResources;
using Microsoft.Practices.Prism.ViewModel;

namespace MusicFmApplication
{
    public class WeatherManager : NotificationObject
    {
        #region WeatherData (INotifyPropertyChanged Property)

        private AsyncProperty<Weather> _weatherData;

        public AsyncProperty<Weather> WeatherData
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

        protected const string CacheName = "Weather";

        protected readonly MainViewModel ViewModel;

        public WeatherManager(MainViewModel viewModel) 
        {
            ViewModel = viewModel;
            //Get Weather Detail
            var task = new Func<Task<Weather>>(() => Task.Run(() =>
            {
                var weather = CityWeatherHelper.GetWeather();
                if (weather.LifeIndexes != null && weather.LifeIndexes.Count > 0 &&
                    !(weather.LifeIndexes is ObservableCollection<LifeIndex>))
                    weather.LifeIndexes = new ObservableCollection<LifeIndex>(weather.LifeIndexes);
                SettingHelper.SetSetting(CacheName, weather.SerializeToString(), ViewModel.AppName);
                return weather;
            }));

            //Get Weather cache from file system
            var cache = SettingHelper.GetSetting(CacheName, ViewModel.AppName).Deserialize<Weather>();
            WeatherData = new AsyncProperty<Weather>(task, cache);
        }
    }
}