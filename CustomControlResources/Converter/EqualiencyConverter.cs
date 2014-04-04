using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CustomControlResources.Converter
{
    public class EqualiencyConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture) 
        {
            return values.Length == 2 && values[0].Equals(values[1]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //public class SubtractionEquationConverter : IMultiValueConverter
    //{
    //    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        if (values == null || values.Length != 2 || parameter == null) return false;
    //        int val;
    //        int.TryParse(parameter.ToString(), out val);
    //        if (values[0] is int && values[1] is int)
    //            return ((int)values[0]) - ((int)values[1]) == (int)parameter;
    //        return false;
    //    }

    //    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
