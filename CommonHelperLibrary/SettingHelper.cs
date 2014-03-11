using System.IO;
using System.Linq;
using System.Xml;

namespace CommonHelperLibrary
{
    public static class SettingHelper
    {
        internal static string ConfigFileName = "App.dat";

        internal static readonly object Locker = new object();

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
            if (appName != "App") ConfigFileName = appName + ".dat";
            var xmlDoc = new XmlDocument();
            //Create config file if not exist
            if (File.Exists(ConfigFileName))
                lock (Locker)
                    xmlDoc.Load(ConfigFileName);
            else
            {
                var node = xmlDoc.CreateNode(XmlNodeType.Element, appName + "Config", "");
                var nodeSettings = xmlDoc.CreateNode(XmlNodeType.Element, "Settings", "");
                node.AppendChild(nodeSettings);
                xmlDoc.AppendChild(node);
                xmlDoc.Save(ConfigFileName);
            }
            return xmlDoc;
        }
    }
}
