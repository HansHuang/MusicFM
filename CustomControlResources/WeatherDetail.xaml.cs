using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
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
            ////Get Weather Detail
            //var task = new Func<Task<Weather>>(() => Task.Run(() => {
            //    var weather = CityWeatherHelper.GetWeather();
            //    if (weather.LifeIndexes != null && weather.LifeIndexes.Count > 0 &&
            //        !(weather.LifeIndexes is ObservableCollection<LifeIndex>))
            //        weather.LifeIndexes = new ObservableCollection<LifeIndex>(weather.LifeIndexes);
            //    return weather;
            //}));
            //WeatherData = new AsyncProperty<Weather>(task);
        }

        #region WeatherData DependencyProperty
        public static readonly DependencyProperty WeatherDataProperty =
            DependencyProperty.Register("WeatherData", typeof(Weather), typeof(WeatherDetail), new PropertyMetadata(default(Weather)));

        public Weather WeatherData
        {
            get { return (Weather)GetValue(WeatherDataProperty); }
            set { SetValue(WeatherDataProperty, value); }
        }
        #endregion
        
    }


    public class Pm25ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is string)) return new SolidColorBrush(Colors.White);
            var number = int.Parse((string)value);
            if (number <= 50) return new SolidColorBrush(Colors.Green);
            if (number <= 100) return new SolidColorBrush(Colors.YellowGreen);
            if (number <= 150) return new SolidColorBrush(Colors.Yellow);
            if (number <= 200) return new SolidColorBrush(Colors.OrangeRed);
            if (number <= 300) return new SolidColorBrush(Colors.Red);
            return new SolidColorBrush(Colors.DarkRed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return "";
        }
    }
}
