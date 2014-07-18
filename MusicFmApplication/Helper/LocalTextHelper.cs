using System.Windows;
using WPFLocalizeExtension.Extensions;

namespace MusicFm.Helper
{
    public static class LocalTextHelper
    {
        #region GetLocText
        /// <summary>
        /// Get Localization Text
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns></returns>
        public static string GetLocText(string key)
        {
            string txt;
            new LocTextExtension(string.Format("Localization:English:{0}", key)).ResolveLocalizedValue(out txt);
            return txt;
        }
        #endregion

        #region AlertError
        /// <summary>
        /// Alert error message box with empry catption
        /// </summary>
        /// <param name="errorKey"></param>
        public static void AlertError(string errorKey)
        {
            var msg = GetLocText(errorKey);
            MessageBox.Show(msg, "", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Alert error message box with message and caption
        /// </summary>
        /// <param name="msgKey"></param>
        /// <param name="captionKey"></param>
        public static void AlertError(string msgKey, string captionKey)
        {
            var msg = GetLocText(msgKey);
            var cap = GetLocText(captionKey);
            MessageBox.Show(msg, cap, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        #endregion

        /// <summary>
        /// Alert Warning message box with message and caption
        /// </summary>
        /// <param name="msgKey"></param>
        /// <param name="captionKey"></param>
        public static void AlertWarning(string msgKey, string captionKey)
        {
            var msg = GetLocText(msgKey);
            var cap = GetLocText(captionKey);
            MessageBox.Show(msg, cap, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    

}
