using System;
using System.Windows.Data;

namespace CustomControlResources.Converter
{
    public class PercentageToDecimalConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) 
        {
            var per = (int) value;
            return (double)per / 100;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
