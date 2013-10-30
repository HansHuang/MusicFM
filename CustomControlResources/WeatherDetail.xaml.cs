using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
            Task.Factory.StartNew(() =>
            {
                var weather = CityWeatherHelper.GetWeather();
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    Weather = weather;
                    if (Weather.LifeIndexes != null && Weather.LifeIndexes.Count > 0 &&
                        !(Weather.LifeIndexes is ObservableCollection<LifeIndex>))
                        Weather.LifeIndexes = new ObservableCollection<LifeIndex>(Weather.LifeIndexes);
                }));
            });
        }

        #region Weather DependencyProperty
        public static readonly DependencyProperty WeatherProperty =
            DependencyProperty.Register("Weather", typeof(Weather), typeof(WeatherDetail), new PropertyMetadata(default(Weather)));

        public Weather Weather
        {
            get { return (Weather)GetValue(WeatherProperty); }
            set { SetValue(WeatherProperty, value); }
        } 
        #endregion

    }
}
