using System;
using System.Windows.Data;

namespace MusicFmApplication.Helper
{
    public class LocalTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value == null || value.Equals(string.Empty)
                       ? string.Empty
                       : LocalTextHelper.GetLocText(value.ToString().Trim());
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.Empty;
        }
    }
}
