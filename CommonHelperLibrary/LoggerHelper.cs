using System;
using System.IO;

namespace CommonHelperLibrary
{
    /// <summary>
    /// Author : Hans Huang
    /// Date : 2014-03-31
    /// Class : LoggerHelper
    /// Discription : Helper class for simple logger
    /// </summary>
    public class LoggerHelper
    {
        public bool IsEnable { get; set; }
        public string AppName { get; set; }
        public string LogDirectory { get; set; }

        private readonly object LogLocker = new object();
        private static LoggerHelper _instance;
        public static LoggerHelper Instance { get { return GetInstance(); } }

        private LoggerHelper() 
        {
            IsEnable = true;
            LogDirectory = Environment.CurrentDirectory + "\\Log\\";
            AppName = "App";
        }

        private static LoggerHelper GetInstance()
        {
            return _instance ?? (_instance = new LoggerHelper());
        }

        /// <summary>
        /// Log Message
        /// </summary>
        /// <param name="title">Message Title</param>
        /// <param name="msg">Message Content</param>
        public void Msg(object title, object msg)
        {
            lock (LogLocker)
            {
                if (!IsEnable) return;
                if (!Directory.Exists(LogDirectory)) Directory.CreateDirectory(LogDirectory);
                var log = LogDirectory + AppName + DateTime.Now.ToString("yyyyMMdd") + ".log";
                using (var sr = new StreamWriter(log, true))
                {
                    sr.Write(string.Format("{0} | {1} | {2}\r\n", title, DateTime.Now, msg));
                }
            }
        }

        /// <summary>
        /// Log Exception
        /// </summary>
        /// <param name="e">Exception</param>
        public void Exception(Exception e)
        {
            Msg("Error", e.Message + "\r\n" + e.StackTrace);
        }
    }
}
