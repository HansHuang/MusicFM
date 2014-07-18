using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommonHelperLibrary;

namespace MusicFm
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public const string Name = "MusicFM";
        public static LoggerHelper Log { get; private set; }

        protected static readonly Mutex Mutex = new Mutex(true, "{6616D937-9F14-493C-B0F9-E342579D8E9E}");

        public App()
        {
            //Onle one instance can running at same time
            if (Mutex.WaitOne(TimeSpan.Zero, true))
                Mutex.ReleaseMutex();
            else
                Shutdown(0);

            Log = LoggerHelper.Instance;
            Log.IsEnable = !string.IsNullOrWhiteSpace(SettingHelper.GetSetting("Log", Name));
            Log.LogDirectory = Environment.CurrentDirectory + "\\Log\\";
            Log.AppName = Name;

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                {
                    //TODO
                };
            DispatcherUnhandledException += (s, e) =>
            {
                e.Handled = true;
                Log.Exception(e.Exception);
            };

            Exit += (s, e) =>
                {
                    //TODO
                };

            InitializeComponent();
        }
    }
}
