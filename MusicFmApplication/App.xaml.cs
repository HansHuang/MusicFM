using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MusicFmApplication
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static readonly Mutex mutex = new Mutex(true, "{6616D937-9F14-493C-B0F9-E342579D8E9E}");
        
        public App()
        {
            //Onle one instance can running at same time
            if (mutex.WaitOne(TimeSpan.Zero, true))
                mutex.ReleaseMutex();
            else
                Shutdown(0);

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                {
                    //TODO
                };

            Exit += (sender, e) =>
                {
                    //TODO
                };

            InitializeComponent();
        }
    }
}
