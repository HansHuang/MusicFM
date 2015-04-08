using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CustomControlResources
{
    public class BooleanClass
    {
        public static bool True
        {
            get { return true; }
        }

        public static bool False
        {
            get { return false; }
        }

        public static bool NotNull(object obj) 
        {
            return obj != null;
        }
    }
}
