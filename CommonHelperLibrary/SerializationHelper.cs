using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;

namespace CommonHelperLibrary
{
    /// <summary>
    /// Provides methods for serialization and De-serialization from objects in Binary Format
    /// </summary>
    public static class SerializationHelper
    {
        #region Serialization
        /// <summary>
        /// Serizlize to byet
        /// </summary>
        /// <param name="graph">object</param>
        /// <returns>byte[]</returns>
        public static byte[] Serialize(this object graph) 
        {
            if (graph == null) return new byte[0];
            byte[] buf;
            var stream = new MemoryStream();
            try {
                new BinaryFormatter().Serialize(stream, graph);
                buf = stream.ToArray();
            }
            catch ( Exception e) {
                buf = new byte[0];
            }
            finally
            {
                stream.Close();
            }
            return buf;
        }

        /// <summary>
        /// Serialize object to string
        /// </summary>
        /// <param name="graph">object</param>
        /// <returns>string</returns>
        public static string SerializeToString(this object graph)
        {
            return Convert.ToBase64String(Serialize(graph));
        }

        /// <summary>
        /// Serialize object to xml
        /// </summary>
        /// <param name="graph">object</param>
        /// <returns>string</returns>
        public static string SerializeToXml(this object graph)
        {
            var writer = new StringWriter();
            string result;
            try
            {
                var serializer = new XmlSerializer(graph.GetType());
                serializer.Serialize(writer, graph);
                result = writer.ToString();
            }
            finally
            {
                writer.Close();
            }

            return result;
        }

        /// <summary>
        /// Serialize object to xml in stream
        /// </summary>
        /// <param name="graph">object</param>
        /// <returns>MemoryStream</returns>
        public static MemoryStream SerializeToXmlInStream(this object graph)
        {
            var stream = new MemoryStream();
            var serializer = new XmlSerializer(graph.GetType());
            serializer.Serialize(stream, graph);
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Serialize object to Json(web)
        /// </summary>
        /// <param name="graph">object</param>
        /// <returns>json string</returns>
        public static string SerializeToJson(this object graph)
        {
            return new JavaScriptSerializer().Serialize(graph);
        }

        /// <summary>
        /// Serialize object to Json(wcf)
        /// </summary>
        /// <param name="graph">object</param>
        /// <returns>json string</returns>
        public static string SerializeToWcfJson(this object graph)
        {
            using (var stream = new MemoryStream())
            {
                new DataContractJsonSerializer(graph.GetType()).WriteObject(stream, graph);
                stream.Position = 0;
                return new StreamReader(stream).ReadToEnd();
            }
        }
        #endregion

        #region Deserialization
        #region Untyped
        /// <summary>
        /// Deserialize to object from string binary
        /// </summary>
        /// <param name="binary">string</param>
        /// <returns>object</returns>
        public static object Deserialize(this string binary)
        {
            if (string.IsNullOrWhiteSpace(binary)) return null;
            return Deserialize(Convert.FromBase64String(binary));
        }

        /// <summary>
        /// Deserialize to object from byte[] binary
        /// </summary>
        /// <param name="binary">byte[]</param>
        /// <returns>object</returns>
        public static object Deserialize(this byte[] binary)
        {
            if (binary == null || binary.Length < 1) return null;
            object obj;
            var formatter = new BinaryFormatter();
            var stream = new MemoryStream(binary);
            try
            {
                obj = formatter.Deserialize(stream);
            }
            finally
            {
                stream.Close();
            }

            return obj;
        }

        /// <summary>
        /// Deserialize to object from xml string (need specify typ)
        /// </summary>
        /// <param name="xml">xml string</param>
        /// <param name="type">Type</param>
        /// <returns>object</returns>
        public static object DeserializeFromXml(this string xml, Type type)
        {
            var serializer = new XmlSerializer(type);
            var bytes = Encoding.UTF8.GetBytes(xml);
            var stream = new MemoryStream(bytes);
            var reader = new XmlTextReader(stream);
            object obj;
            try
            {
                obj = serializer.Deserialize(reader);
            }
            finally
            {
                reader.Close();
            }

            return obj;
        }

        /// <summary>
        /// Deserialize to object from json string
        /// </summary>
        /// <param name="json">string</param>
        /// <returns>object</returns>
        public static object DeserializeFromJson(this string json)
        {
            var ser = new JavaScriptSerializer();
            return ser.DeserializeObject(json);
        }

        /// <summary>
        /// Deserialize to object from wcf json string
        /// </summary>
        /// <param name="json">string</param>
        /// <returns>object</returns>
        public static T DeserializeFromWcfJson<T>(this string json)
        {
            using (new MemoryStream())
                return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(json)));
        }
        #endregion

        #region Typed (Generic)
        /// <summary>
        /// Deserialize to T(Type) from string binary
        /// </summary>
        /// <typeparam name="T">Target Type</typeparam>
        /// <param name="binary">string</param>
        /// <returns>Target type object</returns>
        public static T Deserialize<T>(this string binary)
        {
            try
            {
                return Deserialize<T>(Convert.FromBase64String(binary));
            }
            catch (Exception e)
            {
                LoggerHelper.Instance.Exception(e);
                return default(T);
            }
        }

        /// <summary>
        /// Deserialize to T(Type) from byte[] binary
        /// </summary>
        /// <typeparam name="T">Target Type</typeparam>
        /// <param name="binary">byte[]</param>
        /// <returns>Target type object</returns>
        public static T Deserialize<T>(this byte[] binary)
        {
            var obj=Deserialize(binary);
            if (obj is T) return (T) obj;
            return default(T);
        }

        /// <summary>
        /// Deserialize to T(Type) from xml string
        /// </summary>
        /// <typeparam name="T">Target Type</typeparam>
        /// <param name="xml">xml string</param>
        /// <returns>Target type object</returns>
        public static T DeserializeFromXml<T>(this string xml)
        {
            return (T)DeserializeFromXml(xml, typeof(T));
        }

        /// <summary>
        /// Deserialize to T(Type) from json string
        /// </summary>
        /// <typeparam name="T">Target Type</typeparam>
        /// <param name="json">json string</param>
        /// <returns>Target type object</returns>
        public static T DeserializeFromJson<T>(this string json)
        {
            var ser = new JavaScriptSerializer();
            return ser.Deserialize<T>(json);
        }
        #endregion
        #endregion

    }
}