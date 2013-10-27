using System.IO;
using System.IO.Compression;

namespace CommonHelperLibrary.WEB
{
    public class GZipHelper
    {
        public static byte[] Compress(byte[] data)
        {
            var stream = new MemoryStream();
            var gZipStream = new GZipStream(stream, CompressionMode.Compress);
            gZipStream.Write(data, 0, data.Length);
            gZipStream.Close();
            return stream.ToArray();
        }

        public static byte[] Decompress(Stream stream)
        {
            var stm = new MemoryStream();

            var gZipStream = new GZipStream(stream, CompressionMode.Decompress);

            var bytes = new byte[40960];
            int n;
            while ((n = gZipStream.Read(bytes, 0, bytes.Length)) != 0)
            {
                stm.Write(bytes, 0, n);
            }
            gZipStream.Close();

            return stm.ToArray();
        }

        public static byte[] Decompress(byte[] data)
        {
            return Decompress(new MemoryStream(data));
        }
    }
}
