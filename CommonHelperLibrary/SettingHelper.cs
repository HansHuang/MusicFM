using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace CommonHelperLibrary
{
    /// <summary>
    /// Author : Hans Huang
    /// Date : 2014-03-31
    /// Class : SettingHelper
    /// Discription : Helper class for Config/Setting
    /// </summary>
    public static class SettingHelper
    {
        private static string ConfigFileName = "App.dat";

        private static XmlDocument XmlDoc = null;

        internal static readonly object Locker = new object();

        /// <summary>
        /// Task for save setting
        /// </summary>
        /// <param name="name">Setting Name</param>
        /// <param name="value">Setting Value</param>
        /// <param name="appName">Application Name</param>
        public static async Task SetSettingTask(string name, string value, string appName = "App")
        {
            await Task.Run(() => SetSetting(name, value, appName));
        }

        /// <summary>
        /// Set Setting into config
        /// </summary>
        /// <param name="name">Setting Name</param>
        /// <param name="value">Setting Value</param>
        /// <param name="appName">Application Name</param>
        public static void SetSetting(string name, string value, string appName = "App")
        {
            var xmlDoc = GetConfigXml(appName);
            var settingParentNode = xmlDoc.SelectSingleNode(appName + "Config/Settings");
            if (settingParentNode == null) return;
            var settingNode = settingParentNode.ChildNodes.OfType<XmlNode>().FirstOrDefault(s => s.Name.Equals(name));

            if (settingNode == null)
            {
                var node = xmlDoc.CreateNode(XmlNodeType.Element, name, "");
                node.InnerText = value;
                settingParentNode.AppendChild(node);
            }
            else
                settingNode.InnerText = value;
            lock (Locker)
                xmlDoc.Save(ConfigFileName);
        }

        /// <summary>
        /// Get Setting from config
        /// </summary>
        /// <param name="name">Setting Name</param>
        /// <param name="appName">Application Name</param>
        /// <returns></returns>
        public static string GetSetting(string name, string appName = "App")
        {
            var xmlDoc = GetConfigXml(appName);
            var settingNodes = xmlDoc.SelectSingleNode(appName + "Config/Settings");
            if (settingNodes == null) return string.Empty;

            var setting = settingNodes.ChildNodes.OfType<XmlNode>().FirstOrDefault(s => s.Name.Equals(name));
            return setting == null ? string.Empty : setting.InnerText;
        }


        /// <summary>
        /// Get XmlDocument from config file (will auto gererate if not exists)
        /// </summary>
        /// <param name="appName">Application Name</param>
        /// <returns>XmlDocument</returns>
        private static XmlDocument GetConfigXml(string appName = "App") 
        {
            lock (Locker)
            {
                if (XmlDoc != null) return XmlDoc;

                if (appName != "App") ConfigFileName = appName + ".dat";
                XmlDoc = new XmlDocument();
                //Create config file if not exist
                if (File.Exists(ConfigFileName))
                    XmlDoc.Load(ConfigFileName);
                else
                {
                    var node = XmlDoc.CreateNode(XmlNodeType.Element, appName + "Config", "");
                    var nodeSettings = XmlDoc.CreateNode(XmlNodeType.Element, "Settings", "");
                    node.AppendChild(nodeSettings);
                    XmlDoc.AppendChild(node);
                    XmlDoc.Save(ConfigFileName);
                }
            }
            return XmlDoc;
        }
    }
}
