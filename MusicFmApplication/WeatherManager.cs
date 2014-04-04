using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonHelperLibrary;
using CommonHelperLibrary.WEB;

namespace MusicFmApplication
{
    public class WeatherManager : INotifyPropertyChanged
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

        protected const string CacheName = "Weather";

        //protected readonly MainViewModel ViewModel;

        public WeatherManager(MainViewModel viewModel) 
        {
            //ViewModel = viewModel;

            //Get Weather Detail
            Task.Run(() =>
                {
                    var weather = CityWeatherHelper.GetWeather();
                    if (weather.LifeIndexes != null && weather.LifeIndexes.Count > 0 &&
                        !(weather.LifeIndexes is ObservableCollection<LifeIndex>))
                        weather.LifeIndexes = new ObservableCollection<LifeIndex>(weather.LifeIndexes);
                    viewModel.MainWindow.Dispatcher.InvokeAsync(() =>
                        {
                            WeatherData = weather;
                        });
                    SettingHelper.SetSetting(CacheName, weather.SerializeToString(), App.Name);
                });

            //Get Weather cache from file system
            WeatherData = SettingHelper.GetSetting(CacheName, App.Name).Deserialize<Weather>();
        }
    }
}