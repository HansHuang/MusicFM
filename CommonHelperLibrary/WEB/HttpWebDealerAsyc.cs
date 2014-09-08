using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace CommonHelperLibrary.WEB
{
    /// <summary>
    /// Author : Hans Huang @ Jungo Studio
    /// Date : September 8th, 2014
    /// Class : HttpWebDealerAsyc
    /// Discription : Helper class for dealer with the http website by asyc
    /// </summary>
    public class HttpWebDealerAsyc
    {
        protected static LoggerHelper Logger = LoggerHelper.Instance;

        #region GetJsonObject
        /// <summary>
        /// Get Dynamic Object (json)
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="headers">http request headers</param>
        /// <param name="txtEncoding">The Encoding suggest of response</param>
        /// <returns></returns>
        public static async Task<dynamic> GetJsonObject(string url, WebHeaderCollection headers = null, Encoding txtEncoding = null)
        {
            var json = await GetHtml(url, headers, txtEncoding);
            var jsonSerializer = new JavaScriptSerializer();
            return jsonSerializer.Deserialize<dynamic>(json);
        }
        #endregion

        #region GetHtml
        /// <summary>
        /// Get Html text by URL
        /// I can't tell the encoding of page,So give me the encodeing best
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="headers">http request headers</param>
        /// <param name="txtEncoding">The Encoding suggest of webpage</param>
        /// <returns></returns>
        public static async Task<string> GetHtml(string url, WebHeaderCollection headers = null, Encoding txtEncoding = null)
        {
            var html = "";
            if (string.IsNullOrWhiteSpace(url)) return html;
            var response = await GetResponseByUrl(url, headers);
            html = HttpWebDealerBase.GetHtmlFromResponse(response, txtEncoding);
            return html;
        }

        #endregion

        #region GetResponseByUrl
        /// <summary>
        /// Get response by url
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="headers">Request headers</param>
        /// <returns></returns>
        public static async Task<HttpWebResponse> GetResponseByUrl(string url, WebHeaderCollection headers = null)
        {
            HttpWebResponse response = null;
            var request = (HttpWebRequest)WebRequest.Create(url);
            string postData;
            HttpWebDealerBase.CorrectHeader(request, headers, out postData);

            for (var i = 0; i < 3; i++)
            {
                try
                {
                    await HttpWebDealerBase.TryPostDataAsync(request, postData);
                    response = (HttpWebResponse) await request.GetResponseAsync();
                    return response;
                }
                catch (Exception e)
                {
                    Logger.Msg("Error", url);
                    Logger.Exception(e);
                }
                await Task.Delay(200);
            }
            return response;
        }

        #endregion

        

    }
}
