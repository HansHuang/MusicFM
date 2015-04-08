using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelperLibrary
{
    /// <summary>
    /// Operation with file system helper
    /// </summary>
    public static class FileSystemHelper
    {
        #region WriteFile
        /// <summary>
        /// Write String To File
        /// </summary>
        /// <param name="content">string content</param>
        /// <param name="fileName">File Name</param>
        /// <returns>Success or not</returns>
        public static bool WriteStringToFile(this string content, string fileName)
        {
            if (string.IsNullOrWhiteSpace(content)) return false;
            using (var sr = new StreamWriter(fileName, true))
            {
                sr.Write(content);
            }
            return true;
        }

        /// <summary>
        /// Wirte binary to a file
        /// </summary>
        /// <param name="data">binary data</param>
        /// <param name="fileName"></param>
        public static void WriteFile(this byte[] data, string fileName)
        {
            var fs = new FileStream(fileName, FileMode.Append, FileAccess.Write);
            var bw = new BinaryWriter(fs);
            bw.Write(data, 0, data.Length);
            bw.Close();
            fs.Close();
        } 
        #endregion

        /// <summary>
        /// Read Text from text file
        /// </summary>
        /// <param name="fileName">fileName</param>
        /// <returns>string</returns>
        public static string ReadTextFile(string fileName) {
            var sr = new StreamReader(fileName);
            var content = sr.ReadToEnd();
            sr.Close();
            return content;
        }

        #region CopyFolder
        /// <summary>
        /// Copy the whole folder and its sub-folders & files
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void CopyFolder(string source, string target)
        {
            // Create Target Folder               
            if (!Directory.Exists(target)) Directory.CreateDirectory(target);
            // Copy Files               
            var sourceDir = new DirectoryInfo(source);
            foreach (var file in sourceDir.GetFiles())
            {
                var tFile = target + "\\" + file.Name;
                //if (File.Exists(tFile)) continue;
                file.CopyTo(tFile, true);
            }

            //Loop the sub folder
            foreach (var subDir in sourceDir.GetDirectories())
            {
                CopyFolder(subDir.FullName, target + "//" + subDir.Name);
            }
        }

        #endregion
    }
}
