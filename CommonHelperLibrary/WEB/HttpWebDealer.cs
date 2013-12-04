using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace CommonHelperLibrary.WEB
{
    /// <summary>
    /// Author : Hans.Huang
    /// Date : 2012-12-03
    /// Class : HttpWebDealer
    /// Discription : Helper class for dealer with the http website
    /// </summary>
    public class HttpWebDealer
    {
        #region GetHtml
        /// <summary>
        /// Get Html text by URL
        /// I can't tell the encoding of page,So give me the encodeing best
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="txtEncoding">The Encoding suggest of webpage</param>
        /// <returns></returns>
        public static string GetHtml(string url, Encoding txtEncoding = null)
        {
            var html = "";
            var response = GetResponseByUrl(url);
            if (response == null) return html;
            if (response.ContentEncoding.ToLower().Equals("gzip"))
            {
                if (Equals(txtEncoding, Encoding.GetEncoding("GB2312")))
                    html = Encoding.ASCII.GetString(GZipHelper.Decompress(response.GetResponseStream()));
                else
                    html = Encoding.UTF8.GetString(GZipHelper.Decompress(response.GetResponseStream()));
            }
            else
            {
                txtEncoding = txtEncoding ?? Encoding.Default;
                var sr = new StreamReader(response.GetResponseStream(), txtEncoding); //Encoding.GetEncoding("GB2312")
                html = sr.ReadToEnd();
                sr.Close();
            }
            return html;
        }

        #endregion

        #region GetJsonObject
        /// <summary>
        /// Get Dynamic Object (json)
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="txtEncoding">The Encoding suggest of response</param>
        /// <returns></returns>
        public static dynamic GetJsonObject(string url, Encoding txtEncoding = null)
        {
            var json = GetHtml(url, txtEncoding);
            var jsonSerializer = new JavaScriptSerializer();
            return jsonSerializer.Deserialize<dynamic>(json);
        } 
        #endregion


        #region DownloadFile

        /// <summary>
        /// Get file by HttpWebRequest and save it(Just for Small files those's content length less then 65K)
        /// </summary>
        /// <param name="fileName">File Name to save as</param>
        /// <param name="url">URL</param>
        /// <param name="path">The path to save the file</param>
        /// <param name="timeout">Request timeout</param>
        /// <param name="referenceUrl">Reference URL(To prevent site blocking hotlinking)</param>
        /// <param name="header">Header of request</param>
        /// <returns>Success:Ture</returns>
        public static bool DownloadFile(string fileName, string url, string path, int timeout, string referenceUrl = "")
        {
            var response = GetResponseByUrl(url, referenceUrl, timeout);
            var stream = response.GetResponseStream();
            if (stream == null) return false;
            using (var bReader = new BinaryReader(stream))
            {
                var length = Int32.Parse(response.ContentLength.ToString(CultureInfo.InvariantCulture));
                var byteArr = new byte[length];
                //stream.Read(byteArr, 0, length);
                bReader.Read(byteArr, 0, length);
                //if (File.Exists(path + fileName)) File.Delete(path + fileName);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                var fs = File.Create(path +"\\"+ fileName);
                fs.Write(byteArr, 0, length);
                fs.Close();
            }
            return true;
        }


        /// <summary>
        /// Get file by WebClient method
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="url"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool DownloadFile(string fileName, string url, string path)
        {
            using (var wc = new WebClient())
            {
                //if (File.Exists(path +"\\"+ fileName)) File.Delete(path +"\\"+ fileName);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                wc.DownloadFile(url, path + "\\" + fileName);
            }
            return true;
        }

        #endregion


        #region GetResponseByUrl

        /// <summary>
        /// Get response by url
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="reference">Reference URL(To prevent site blocking hotlinking)</param>
        /// <param name="requestTimeout">Request timeout(Set to 0 for no limit)</param>
        /// <returns></returns>
        public static HttpWebResponse GetResponseByUrl(string url, string reference, int requestTimeout = 0)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            if (string.IsNullOrEmpty(reference))
                request.Referer = reference;
            //if (header != null)
            //{
            //    request.UserAgent = header[HttpRequestHeader.UserAgent];
            //    request.Connection = header[HttpRequestHeader.Connection];
            //    request.Accept = header[HttpRequestHeader.Accept];
            //}
            if (requestTimeout > 0)
                request.Timeout = requestTimeout;

            var count = 0;
            HttpWebResponse response = null;
            while (count < 3)
            {
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                    return response;
                }
                catch (Exception)
                {
                    count++;
                    Thread.Sleep(3000);
                }
            }
            return response;
        }

        public static HttpWebResponse GetResponseByUrl(string url)
        {
            //var header = new WebHeaderCollection();
            //header.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:25.0) Gecko/20100101 Firefox/25.0");
            //header.Add(HttpRequestHeader.Connection, "keep-alive");
            return GetResponseByUrl(url, string.Empty);
        }

        #endregion



        //http://www.cnblogs.com/LoveJenny/archive/2011/12/02/2271543.html
        //http://www.yongfa365.com/Item/GetThumbnailImage-DrawString-DrawImageUnscaled.html

        /*Tips : Get content info after Login website
         * 
         * Append a cookie to http request
         * 
         * http://www.cnblogs.com/jannock/archive/2008/09/05/1285191.html
         * 
         */

    }
}
