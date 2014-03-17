using System;
using System.Windows.Data;

namespace CustomControlResources.Converter
{
    public class InverseBoolConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) 
        {
            if (!(value is bool)) return false;
            return !(bool) value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
