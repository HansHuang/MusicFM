using System;
using System.Text;
using CommonHelperLibrary;

namespace Service
{
    /// <summary>
    /// AES Cryptograhic for baidu music(Ting)
    /// </summary>
    public static class AESForTing
    {
        private const string Iv = "2011121211141020";
        private const string Key = "A93F8A4C68D83F7A";

        static AESForTing()
        {
            //var key = AESHelper.Md5Hash("6461772803151020");
            //_key = key.Substring(key.Length - 16).ToUpper();
        }

        public static string Encrypt(string text)
        {
            var encode = new ASCIIEncoding();
            var encrypted = AESHelper.EncryptStringToBytes(text, encode.GetBytes(Key), encode.GetBytes(Iv));
            return Uri.EscapeDataString(Convert.ToBase64String(encrypted));
        }
    }
}

