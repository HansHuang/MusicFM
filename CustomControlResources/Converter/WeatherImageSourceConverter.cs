using System;
using System.Windows.Data;

namespace CustomControlResources.Converter
{
    public class WeatherImageSourceConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converter the image number to imageresource
        /// </summary>
        /// <param name="values">The first is image id, the second is resource dictionary</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">B--Big, S--Small. D--Daytime, N--Night</param>
        /// <param name="culture"></param>
        /// <returns>imageresource</returns>
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var imageId = values[0] as string;
            var allResources = values[1] as System.Windows.ResourceDictionary;
            var par = ((string)parameter).ToLower();
            if (string.IsNullOrEmpty(imageId) || allResources == null || allResources.Count == 0) return null;

            if (imageId == "99" && values.Length > 2 && values[2] is string) imageId = (string) values[2];

            bool isDaytime;
            if (par.Length < 2) isDaytime = DateTime.Now.Hour < 18;
            else isDaytime = par.Contains("d");
            var size = par.Contains("s") ? "Small" : "Big";
            var sourceString = string.Format("{0}{1}{2}", size, isDaytime ? "D" : "N", imageId);
            return allResources[sourceString];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
