using System;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Media;

namespace CustomControlResources.Converter
{
    public class LyricHighLightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var color = values[0].Equals(values[1]) ? Colors.YellowGreen : Colors.Silver;
            return new SolidColorBrush(color);
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
