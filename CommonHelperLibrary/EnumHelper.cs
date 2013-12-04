using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace CommonHelperLibrary
{
    public static class EnumHelper
    {
        public static string GetEnumDescription<T>(T enumeratedType)
        {
            var description = enumeratedType.ToString();

            var enumType = typeof(T);
            // Can't use type constraints on value types, so have to do check like this
            if (enumType.BaseType != typeof(Enum))
                throw new ArgumentException("T must be of type System.Enum");

            var fieldInfo = enumeratedType.GetType().GetField(enumeratedType.ToString());

            if (fieldInfo != null)
            {
                var attribues = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attribues.Length > 0)
                {
                    description = ((DescriptionAttribute)attribues[0]).Description;
                }
            }

            return description;
        }

        public static string GetEnumCollectionDescription<T>(Collection<T> enums)
        {
            var sb = new StringBuilder();

            var enumType = typeof(T);
            // Can't use type constraints on value types, so have to do check like this
            if (enumType.BaseType != typeof(Enum))
                throw new ArgumentException("T must be of type System.Enum");

            foreach (T enumeratedType in enums)
            {
                sb.AppendLine(GetEnumDescription(enumeratedType));
            }

            return sb.ToString();
        }
    }
}