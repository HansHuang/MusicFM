using System;
using System.Windows;
using System.Windows.Data;

namespace CustomControlResources.Converter
{
    public class EqualiencyToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values == null || values.Length < 1 || values[0] == null) return Visibility.Collapsed;
            bool isEqual;
            if (values.Length == 1 && parameter != null)
                isEqual = values[0].ToString().Equals(parameter);
            else
                isEqual = values[0].Equals(values[1]);

            return isEqual ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
