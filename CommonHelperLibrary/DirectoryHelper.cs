using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelperLibrary
{
    public class DirectoryHelper
    {
        /// <summary>
        /// Get the size(in Byte) of the whole directory
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static long DirectorySize(string path)
        {
            // 1. Get array of all file names.
            var files = Directory.GetFiles(path, "*.*");

            // 2. Calculate total bytes of all files.
            return files.Sum(s => new FileInfo(s).Length);
        }

        /// <summary>
        /// Make sure the directory is exist. If not, create it.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool MakeSureExist(string path)
        {
            try
            {
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
