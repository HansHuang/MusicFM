using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CommonHelperLibrary
{
    public static class IeCookieHelper
    {
        [DllImport("wininet.dll", SetLastError = true)]
        public static extern bool InternetGetCookieEx(
            string url,
            string cookieName,
            StringBuilder cookieData,
            ref int size,
            Int32 dwFlags,
            IntPtr lpReserved);

        private const Int32 InternetCookieHttponly = 0x2000;
        private const int InternetOptionEndBrowserSession = 42;

        /// <summary>
        /// Gets the url cookie data(with session)
        /// </summary>
        /// <param name="url">The url</param>
        /// <returns></returns>
        public static string GetCookieData(string url)
        {
            // Determine the size of the cookie
            var datasize = 8192 * 16;
            var cookieData = new StringBuilder(datasize);
            if (!InternetGetCookieEx(url, null, cookieData, ref datasize, InternetCookieHttponly, IntPtr.Zero))
            {
                if (datasize < 0)
                    return null;
                // Allocate stringbuilder large enough to hold the cookie
                cookieData = new StringBuilder(datasize);
                if (!InternetGetCookieEx(
                    url,
                    null, cookieData,
                    ref datasize,
                    InternetCookieHttponly,
                    IntPtr.Zero))
                    return null;
            }
            return cookieData.ToString();
        }

        //public static string GetSessionData(string url)
        //{
        //    var datasize = 8192 * 16;
        //    var cookieData = new StringBuilder(datasize);
        //    if (!InternetGetCookieEx(url, null, cookieData, ref datasize, InternetOptionEndBrowserSession, IntPtr.Zero))
        //    {
        //        if (datasize < 0)
        //            return null;
        //        // Allocate stringbuilder large enough to hold the cookie
        //        cookieData = new StringBuilder(datasize);
        //        if (!InternetGetCookieEx(url, null, cookieData, ref datasize, InternetOptionEndBrowserSession, IntPtr.Zero))
        //            return null;
        //    }
        //    return cookieData.ToString();
        //}
    }
}
