using System;
using System.Text;
using System.Text.RegularExpressions;

namespace CommonHelperLibrary.WEB
{
    /// <summary>
    /// Author : Hans.Huang
    /// Date : 2013-03-16
    /// Class : CityIPHelper
    /// Discription : Helper class for current clent's city & ip
    /// </summary>
    public static class CityIPHelper
    {
        #region GetCurrentCity
        /// <summary>
        /// According to public IP get current China City
        /// </summary>
        public static string GetCurrentCity()
        {
            //1. Get html from ip server
            var html = GetHtmlFromIPServer();

            //2. Get city name from html with regular expression
            var r = new Regex(@"来自：(.+市)");
            var city = r.Match(html).Groups[1].Value.Trim();
            if (!city.Contains("省") && html.Contains("(") && html.Contains(")"))
                city = html.Substring(html.IndexOf("(", StringComparison.Ordinal) + 1,
                              html.IndexOf(")", StringComparison.Ordinal) - html.IndexOf("(", StringComparison.Ordinal) - 1);
            return city;
        } 
        #endregion

        #region GetPublicIP
        /// <summary>
        /// Get Current PC Public IP Address
        /// </summary>
        /// <returns>IP Address(xxx.xxx.xxx.xxx)</returns>
        public static string GetPublicIP()
        {
            //1. Get html from ip server
            var html = GetHtmlFromIPServer();

            //2. Select IP address from html with regular expression
            var ip = html.Substring(html.IndexOf("[", StringComparison.Ordinal) + 1,
                              html.IndexOf("]", StringComparison.Ordinal) - html.IndexOf("[", StringComparison.Ordinal) - 1);
            return ip;
        } 
        #endregion

        #region GetHtmlFromIPServer(Private)
        /// <summary>
        /// Get Response Html From IP Server
        /// </summary>
        /// <returns>HTML</returns>
        private static string GetHtmlFromIPServer()
        {
            /*
             * Get html from IP server
             * ip138 is China server, it offer public ip address, city, Network operator
             * If your computer is in China, it can provide the most accurate data
             * 
             * http://checkip.dyndns.org is available, too. 
             * It is more suitable for other contries but China.
             * Due to China's national conditions, its data is not accurate everytime             * 
             * http://int.dpool.sina.com.cn/iplookup/iplookup.php?format=js
             * 
             * Old api, seems not working now
             * http://iframe.ip138.com/ic.asp
             * 
             */
            return HttpWebDealer.GetHtml("http://1111.ip138.com/ic.asp", null, Encoding.Default);
        }

        #endregion
    }

    
}
