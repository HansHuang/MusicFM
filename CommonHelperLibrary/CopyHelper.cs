using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelperLibrary
{
    /// <summary>
    /// Author : Hans Huang
    /// Date : 2014-06-23
    /// Class : CopyHelper
    /// Discription : Helper for copy /clone class
    /// </summary>
    public static class CopyHelper
    {
        /// <summary>
        /// Copy value for each property(compare type and name)
        /// </summary>
        /// <typeparam name="T">target type</typeparam>
        /// <typeparam name="S">source type</typeparam>
        /// <param name="target">target</param>
        /// <param name="source">source</param>
        /// <param name="isDeep">Is deep copy or not, for reference value</param>
        /// <returns></returns>
        public static T Copy<T, S>(this T target, S source, bool isDeep = false)
            where T : class,new()
            where S : class
        {
            if (target == null) target = new T();
            if (source == null) return target;

            var tProperties = typeof (T).GetProperties();
            var sProperties = typeof (S).GetProperties();
            foreach (var sPro in sProperties)
            {
                //Type and name are same
                var tPro = tProperties.FirstOrDefault(s => s.PropertyType == sPro.PropertyType && s.Name == sPro.Name);
                if (tPro == null) continue;
                var value = sPro.GetValue(source);
                if (isDeep && value != null && !value.GetType().IsValueType)
                    tPro.SetValue(target, value.Serialize().Deserialize());
                else
                    tPro.SetValue(target, value);
            }
            return target;
        }
    }
}
