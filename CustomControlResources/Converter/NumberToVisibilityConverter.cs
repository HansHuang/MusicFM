using System;
using System.Windows;
using System.Windows.Data;

namespace CustomControlResources.Converter
{
    public class NumberToVisibilityConverter:IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) 
        {
            var num = System.Convert.ToDouble(value);
            return num > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
