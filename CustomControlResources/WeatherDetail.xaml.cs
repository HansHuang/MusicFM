using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CommonHelperLibrary;
using CommonHelperLibrary.WEB;

namespace CustomControlResources
{
    /// <summary>
    /// Interaction logic for WeatherDetail.xaml
    /// </summary>
    public partial class WeatherDetail : UserControl
    {
        public WeatherDetail()
        {
            InitializeComponent();
            //Get Weather Detail
            var task = new Func<Task<Weather>>(() => Task.Run(() => {
                var weather = CityWeatherHelper.GetWeather();
                if (weather.LifeIndexes != null && weather.LifeIndexes.Count > 0 &&
                    !(weather.LifeIndexes is ObservableCollection<LifeIndex>))
                    weather.LifeIndexes = new ObservableCollection<LifeIndex>(weather.LifeIndexes);
                return weather;
            }));
            WeatherData = new AsyncProperty<Weather>(task);
        }

        #region WeatherData DependencyProperty
        public static readonly DependencyProperty WeatherDataProperty =
            DependencyProperty.Register("WeatherData", typeof(AsyncProperty<Weather>), typeof(WeatherDetail), new PropertyMetadata(default(AsyncProperty<Weather>)));

        public AsyncProperty<Weather> WeatherData
        {
            get { return (AsyncProperty<Weather>)GetValue(WeatherDataProperty); }
            set { SetValue(WeatherDataProperty, value); }
        } 
        #endregion

    }
}
