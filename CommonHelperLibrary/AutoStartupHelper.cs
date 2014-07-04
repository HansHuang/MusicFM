using Microsoft.Win32;

namespace CommonHelperLibrary
{
    /// <summary>
    /// Author : Hans Huang
    /// Date : 2014-07-04
    /// Class : AutoStartupHelper
    /// Discription : set programe auto startup with windows
    /// </summary>
    public class AutoStartupHelper
    {
        //private const string Currentversion = @"Software\Microsoft\Windows\CurrentVersion";
        private const string RunLocation = @"Software\Microsoft\Windows\CurrentVersion\Run";
        //private const string AppLocation = @"Software\Microsoft\Windows\CurrentVersion\App Paths";


        /// <summary>
        /// Sets the autostart value for the assembly.
        /// </summary>
        /// <param name="keyName">Registry Key Name</param>
        /// <param name="assemblyLocation">Assembly location (e.g. Assembly.GetExecutingAssembly().Location)</param>
        public static void SetAutoStart(string keyName, string assemblyLocation)
        {
            var key = Registry.CurrentUser.CreateSubKey(RunLocation);
            if (key == null) return;
            key.SetValue(keyName, assemblyLocation);
            key.Flush();
        }

        /// <summary>
        /// Returns whether auto start is enabled.
        /// </summary>
        /// <param name="keyName">Registry Key Name</param>
        /// <param name="assemblyLocation">Assembly location (e.g. Assembly.GetExecutingAssembly().Location)</param>
        public static bool IsAutoStartEnabled(string keyName, string assemblyLocation)
        {
            var key = Registry.CurrentUser.OpenSubKey(RunLocation);
            if (key == null) return false;

            var value = (string)key.GetValue(keyName);
            if (value == null) return false;

            return (value == assemblyLocation);
        }

        /// <summary>
        /// Unsets the autostart value for the assembly.
        /// </summary>
        /// <param name="keyName">Registry Key Name</param>
        public static void UnSetAutoStart(string keyName)
        {
            var key = Registry.CurrentUser.CreateSubKey(RunLocation);
            if (key != null) key.DeleteValue(keyName);
        }
    }
}
