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
            if (values == null || values.Length < 1 || values[0] == null) return false;
            bool isEqual;
            if (values.Length == 1 && parameter != null)
                isEqual = values[0].ToString() == parameter.ToString();
            else
                isEqual = values[0].Equals(values[1]);
            return isEqual;
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
