using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CommonHelperLibrary
{
    /// <summary>
    /// Author : Hans Huang @ Jungo Studio
    /// Date : September 8th, 2014
    /// Class : VolumeHelper
    /// Discription : Control the volumn of system
    /// </summary>
    public static class VolumeHelper
    {
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        /// <summary>
        /// Set volume mute
        /// </summary>
        public static void Mute()
        {
            keybd_event((byte) Key.VolumeMute, 0, 0, 0);
        }

        /// <summary>
        /// Set volume one step up
        /// </summary>
        public static void Up()
        {
            keybd_event((byte)Key.VolumeUp, 0, 0, 0);
        }

        /// <summary>
        /// Set volume one step down
        /// </summary>
        public static void Down()
        {
            keybd_event((byte)Key.VolumeDown, 0, 0, 0);
        }
    }
}
