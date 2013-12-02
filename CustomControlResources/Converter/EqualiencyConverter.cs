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
            //Console.WriteLine("Values[0]: " + values[0].GetType() + "--" + values[0]);
            //Console.WriteLine("Values[1]: " + values[1].GetType() + "--" + values[1]);
            //Console.WriteLine(values[0].Equals(values[1]));
            return values.Length == 2 && values[0].Equals(values[1]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
