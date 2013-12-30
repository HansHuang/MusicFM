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
    public static class AnalysisFlvAddress
    {
        public static string FlvUrl(this string websideUrl)
        {
            if (string.IsNullOrWhiteSpace(websideUrl)) return string.Empty;

            var webClient = new WebClient
            {
                Headers = new WebHeaderCollection {
          {
            "Cookie",
            @"bdshare_firstime=1374736899867; PHPSESSID=espr790j6eelgophvb3jnnt4f1; parse_time=; 
              pianhao=%7B%22qing%22%3A%22super%22%2C%22qtudou%22%3A%22null%22%2C%22qyouku%22%3A%22
              null%22%2C%22q56%22%3A%22null%22%2C%22qcntv%22%3A%22null%22%2C%22qletv%22%3A%22null
              %22%2C%22qqiyi%22%3A%22null%22%2C%22qsohu%22%3A%22null%22%2C%22qqq%22%3A%22null%22%2C%22
              qku6%22%3A%22null%22%2C%22qyinyuetai%22%3A%22null%22%2C%22qtangdou%22%3A%22null%22%2C%22
              qxunlei%22%3A%22null%22%2C%22qfunshion%22%3A%22null%22%2C%22qsina%22%3A%22null%22%2C%22
              qpptv%22%3A%22null%22%2C%22qpps%22%3A%22null%22%2C%22xia%22%3A%22ask%22%2C%22pop%22%3A%
              22no%22%2C%22open%22%3A%22no%22%7D"
          }
        }
            };
            var requestUrl = "http://www.flvcd.com/parse.php?format=&kw=" + System.Web.HttpUtility.UrlEncode(websideUrl);
            var html = webClient.DownloadString(requestUrl);

            var url = html.Split(new[] { "clipurl", "cliptitle" }, StringSplitOptions.RemoveEmptyEntries)[1];
            url = url.Split(new[] { '\"' })[1];
            return url;
        }
    }
}
