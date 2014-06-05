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
            if (values == null || values.Length < 1 || values[0] == null) return Visibility.Collapsed;
            bool isEqual;
            if (values.Length == 1 && parameter != null)
                isEqual = values[0].ToString().Equals(parameter);
            else
                isEqual = values[0].Equals(values[1]);
            return isEqual ? Visibility.Collapsed : Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
