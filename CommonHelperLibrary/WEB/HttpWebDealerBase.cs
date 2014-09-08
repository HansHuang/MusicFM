using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelperLibrary.WEB
{
    internal class HttpWebDealerBase
    {
        internal static void CorrectHeader(HttpWebRequest request, WebHeaderCollection headers, out string postData)
        {
            postData = string.Empty;
            if (request == null || headers == null) return;
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
                else if (string.Equals(header, "user-agent", StringComparison.OrdinalIgnoreCase))
                {
                    toRemove.Add(header);
                    request.UserAgent = value.FirstOrDefault();
                }
                else if (header == "ContentType")
                {
                    toRemove.Add(header);
                    request.ContentType = value.FirstOrDefault();
                }
                else if (header == "Method")
                {
                    toRemove.Add(header);
                    request.Method = value.FirstOrDefault();
                }
                else if (header == "PostData")
                {
                    toRemove.Add(header);
                    postData = value.FirstOrDefault();
                }
                //else if()
            }
            toRemove.ForEach(headers.Remove);
            if (headers.Count > 0) request.Headers = headers;
        }

        internal static async Task TryPostDataAsync(HttpWebRequest request, string postData)
        {
            if (request.Method == "POST" && !string.IsNullOrWhiteSpace(postData))
            {
                var buffer = Encoding.ASCII.GetBytes(postData);
                request.ContentLength = buffer.Length;
                var stream = await request.GetRequestStreamAsync();
                stream.Write(buffer, 0, buffer.Length);
                stream.Close();
            }
        }

        internal static void TryPostData(HttpWebRequest request, string postData)
        {
            if (request.Method == "POST" && !string.IsNullOrWhiteSpace(postData))
            {
                var buffer = Encoding.ASCII.GetBytes(postData);
                request.ContentLength = buffer.Length;
                var stream = request.GetRequestStream();
                stream.Write(buffer, 0, buffer.Length);
                stream.Close();
            }
        }

        internal static string GetHtmlFromResponse(HttpWebResponse response, Encoding txtEncoding = null)
        {
            var html = string.Empty;
            if (response == null) return html;
            using (var stream = response.GetResponseStream())
            {
                if (response.ContentEncoding.ToLower().Equals("gzip"))
                {
                    if (Equals(txtEncoding, Encoding.GetEncoding("GB2312")))
                        html = Encoding.ASCII.GetString(GZipHelper.Decompress(stream));
                    else
                        html = Encoding.UTF8.GetString(GZipHelper.Decompress(stream));
                }
                else
                {
                    StreamReader sr;
                    if (txtEncoding == null)
                        sr = new StreamReader(stream, true);
                    else
                        sr = new StreamReader(stream, txtEncoding);
                    html = sr.ReadToEnd();
                    sr.Close();
                }
            }
            return html;
        }
    }
}
