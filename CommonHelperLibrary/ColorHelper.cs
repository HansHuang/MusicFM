using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CommonHelperLibrary
{
    /// <summary>
    /// Author : Hans Huang
    /// Date : 2014-06-23
    /// Class : ColorHelper
    /// Discription : Helper for color
    /// </summary>
    public static class ColorHelper
    {
        public static Color GetRandomColor()
        {
            var rdm = new Random();
            return Color.FromArgb(215, (byte)rdm.Next(0, 150), (byte)rdm.Next(0, 150), (byte)rdm.Next(0, 150));
        }
    }
}
