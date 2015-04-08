using System;
using System.Windows;
using CommonHelperLibrary;

namespace MvPlayer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string Name = "MusicMV";
        public static LoggerHelper Log { get; private set; }

        public App()
        {
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
