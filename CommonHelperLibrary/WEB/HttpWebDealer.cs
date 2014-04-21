using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        /// <param name="headers">http request headers</param>
        /// <param name="txtEncoding">The Encoding suggest of webpage</param>
        /// <returns></returns>
        public static string GetHtml(string url, WebHeaderCollection headers = null, Encoding txtEncoding = null)
        {
            var html = "";
            var response = GetResponseByUrl(url,headers);
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
        /// <param name="headers">http request headers</param>
        /// <param name="txtEncoding">The Encoding suggest of response</param>
        /// <returns></returns>
        public static dynamic GetJsonObject(string url,WebHeaderCollection headers = null, Encoding txtEncoding = null)
        {
            var json = GetHtml(url, headers, txtEncoding);
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
        /// <param name="headers">http request header</param>
        /// <returns>Success:Ture</returns>
        public static bool DownloadFile(string fileName, string url, string path, int timeout, WebHeaderCollection headers = null)
        {
            var response = GetResponseByUrl(url, headers, timeout);
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
        /// <param name="fileName">File name to save</param>
        /// <param name="url">file url to download</param>
        /// <param name="path">file path to save</param>
        /// <param name="monitor">Monitor action for download progress</param>
        /// <returns></returns>
        public static bool DownloadFile(string fileName, string url, string path,
                                        DownloadProgressChangedEventHandler monitor = null)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            using (var wc = new WebClient())
            {
                //if (File.Exists(path +"\\"+ fileName)) File.Delete(path +"\\"+ fileName);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                if (monitor != null)
                    wc.DownloadProgressChanged += monitor;
                
                wc.DownloadFileAsync(new Uri(url), path + "\\" + fileName);
            }
            return true;
        }

        /// <summary>
        /// Download largest file from url list
        /// </summary>
        /// <param name="fileName">File name to save</param>
        /// <param name="urls">file url list to download</param>
        /// <param name="path">file path to save</param>
        /// <param name="monitor">Monitor action for download progress</param>
        /// <returns>File Length</returns>
        public static long DownloadLargestFile(string fileName, List<string> urls, string path,
                                               DownloadProgressChangedEventHandler monitor = null)
        {
            if (urls == null || urls.Count == 0) return 0;
            var results = new Dictionary<string, long>();
            var signals = new List<EventWaitHandle>();
            var locker = new object();
            foreach (var url in urls)
            {
                var signal = new EventWaitHandle(true, EventResetMode.ManualReset);
                signals.Add(signal);
                var str = url;
                signal.Reset();
                Task.Run(() =>
                {
                    var rsponse = GetResponseByUrl(str, null, 3000);
                    if (rsponse == null) return;
                    lock (locker)
                    {
                        results.Add(str, rsponse.ContentLength);
                    }
                    signal.Set();
                });
            }
            signals.ForEach(s => s.WaitOne(3000));
            var largest = new KeyValuePair<string, long>(string.Empty, 0);
            foreach (var res in results)
            {
                if (res.Value > largest.Value) largest = res;
            }

            return DownloadFile(fileName, largest.Key, path, monitor) ? largest.Value : 0;
        }

        #endregion


        #region GetResponseByUrl

        /// <summary>
        /// Get response by url
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="headers">Request headers</param>
        /// <param name="requestTimeout">Request timeout(Set to 0 for no limit)</param>
        /// <returns></returns>
        public static HttpWebResponse GetResponseByUrl(string url, WebHeaderCollection headers = null, int requestTimeout = 0)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            if (headers != null)
            {
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:26.0) Gecko/20100101 Firefox/26.0";
                //http://stackoverflow.com/questions/239725/cannot-set-some-http-headers-when-using-system-net-webrequest
                var toRemove = new List<string>();
                for (var i = 0; i < headers.Count; ++i)
                {
                    var header = headers.GetKey(i);
                    var value = headers.GetValues(i);
                    if (string.IsNullOrWhiteSpace(header) || value == null || value.Length < 1) continue;
                    if (header == "Referer")
                    {
                        toRemove.Add(header);
                        request.Referer = value.Aggregate((s, t) => s + "; " + t);
                    }
                    else if (string.Equals(header, "user-agent",StringComparison.OrdinalIgnoreCase))
                    {
                        toRemove.Add(header);
                        request.UserAgent = value.FirstOrDefault();
                    }
                    //else if()
                }
                toRemove.ForEach(headers.Remove);
                if (headers.Count > 0) request.Headers = headers;
            }
            if (requestTimeout > 0) request.Timeout = requestTimeout;

            var count = 0;
            HttpWebResponse response = null;
            while (count < 3)
            {
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                    return response;
                }
                catch (Exception e) 
                {
                    LoggerHelper.Instance.Msg("Error", url);
                    LoggerHelper.Instance.Exception(e);
                    count++;
                    Thread.Sleep(200);
                }
            }
            return response;
        }

        #endregion



        //http://www.cnblogs.com/LoveJenny/archive/2011/12/02/2271543.html
        //http://www.yongfa365.com/Item/GetThumbnailImage-DrawString-DrawImageUnscaled.html

    }
}
