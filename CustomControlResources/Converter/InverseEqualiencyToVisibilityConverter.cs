using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace CustomControlResources.Converter
{
    public class InverseEqualiencyToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values == null || values.Length < 1) return Visibility.Collapsed;
            bool isEqual;
            if (values.Length == 1 && parameter != null && values[0] != null)
                isEqual = values[0].ToString().Equals(parameter);
            else
                isEqual = Equals(values[0],values[1]);
            return isEqual ? Visibility.Collapsed : Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
