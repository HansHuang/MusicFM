using System;
using System.Windows.Data;

namespace MusicFmApplication.Helper
{
    public class LocalTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return string.Empty;
            var title = value.ToString();
            if (string.IsNullOrWhiteSpace(title)) return string.Empty;
            return LocalTextHelper.GetLocText(title.Trim());
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.Empty;
        }
    }
}
