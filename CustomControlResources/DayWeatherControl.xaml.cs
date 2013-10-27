using System.Windows;
using System.Windows.Controls;
using CommonHelperLibrary.WEB;

namespace CustomControlResources
{
    /// <summary>
    /// Interaction logic for DayWeatherControl.xaml
    /// </summary>
    public partial class DayWeatherControl : UserControl
    {
        public DayWeatherControl()
        {
            InitializeComponent();
        }

        #region DayWeather (DependencyProperty)

        public static readonly DependencyProperty DayWeatherProperty =
            DependencyProperty.Register("DayWeather", typeof(WeatherBase), typeof(DayWeatherControl), new PropertyMetadata(default(WeatherBase)));

        public WeatherBase DayWeather
        {
            get { return (WeatherBase)GetValue(DayWeatherProperty); }
            set { SetValue(DayWeatherProperty, value); }
        }

        #endregion
    }
}
