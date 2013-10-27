﻿using System;
using System.Windows.Data;

namespace CustomControlResources.Converter
{
    public class InverseBoolConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) 
        {
            return !(bool) value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
