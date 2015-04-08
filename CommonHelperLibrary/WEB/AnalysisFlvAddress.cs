using System;
using System.Net;

namespace CommonHelperLibrary.WEB
{
    /// /// <summary>
    /// Author : Hans Huang
    /// Date : 2013-12-27
    /// Class : AnalysisFlvAddress
    /// Discription : Analysis the flv video real address from webpage
    /// </summary>
    public static class AnalysisVideoAddress
    {
        /// <summary>
        /// Get Flv real url from its play webpage
        /// </summary>
        /// <param name="webpageUrl">Play Webpage url</param>
        /// <param name="level">Video resolution(when is avaliable): 1-Nomal420P; 2-540P; 3-HD 720P(default)</param>
        /// <returns></returns>
        public static string FlvUrl(this string webpageUrl, byte level = 3)
        {
            if (string.IsNullOrWhiteSpace(webpageUrl)) return string.Empty;
            string rsl;
            switch (level)
            {
                case 1:
                    rsl = "normal";
                    break;
                case 2:
                    rsl = "high";
                    break;
                default:
                    rsl = "super";
                    break;
            }

            var webClient = new WebClient
            {
                Headers = new WebHeaderCollection {
                    {
                        "Cookie",
                        "pianhao=%7B%22qing%22%3A%22" + rsl +
                        "%22%2C%22qtudou%22%3A%22null%22%2C%22qyouku%22%3A%22null%22%2C%22q56%22%3A%22null%22%2C%22qcntv%22%3A%22null%22%2C%22qletv%22%3A%22null%22%2C%22qqiyi%22%3A%22null%22%2C%22qsohu%22%3A%22null%22%2C%22qqq%22%3A%22null%22%2C%22qku6%22%3A%22null%22%2C%22qyinyuetai%22%3A%22null%22%2C%22qtangdou%22%3A%22null%22%2C%22qxunlei%22%3A%22null%22%2C%22qfunshion%22%3A%22null%22%2C%22qsina%22%3A%22null%22%2C%22qpptv%22%3A%22null%22%2C%22qpps%22%3A%22null%22%2C%22xia%22%3A%22ask%22%2C%22pop%22%3A%22no%22%2C%22open%22%3A%22no%22%7D;"
                    },
                    {"User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:26.0) Gecko/20100101 Firefox/26.0"}
                }
            };
            var requestUrl = "http://www.flvcd.com/parse.php?format=&kw=" + System.Web.HttpUtility.UrlEncode(webpageUrl);
            var html = webClient.DownloadString(requestUrl);

            var url = html.Split(new[] { "clipurl", "cliptitle" }, StringSplitOptions.RemoveEmptyEntries)[1];
            url = url.Split(new[] { '\"' })[1];
            return url;
        }
    }
}
