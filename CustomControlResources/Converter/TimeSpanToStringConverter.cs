using System;
using System.Windows.Data;

namespace CustomControlResources.Converter
{
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) 
        {
            var ts = (TimeSpan) value;
            string result;
            if (ts.Hours > 0)
                result = ts.ToString(@"hh\:mm\:ss");
            else if (ts.TotalSeconds > 0)
                result = ts.ToString(@"mm\:ss");
            else
                result = "--";

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
