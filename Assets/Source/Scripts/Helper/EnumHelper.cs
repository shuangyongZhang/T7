

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
namespace LB
{


    public static class EnumHelper
    {
        public static string GetDescription(this Enum val)
        {
            var field = val.GetType().GetField(val.ToString());
            var customAttribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return customAttribute == null ? val.ToString() : ((DescriptionAttribute)customAttribute).Description;
        }

        public static List<string> GetDescriptions(Type enumType)
        {
            List<string> list = new List<string>();
            FieldInfo[] fieldinfos = enumType.GetFields();
            foreach (FieldInfo field in fieldinfos)
            {
                if (field.FieldType.IsEnum)
                {
                    Object[] objs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);

                    list.Add(((DescriptionAttribute)objs[0]).Description);
                }

            }
            return list;

        }

        public static T GetValue<T>(this string value) where T : struct
        {

            if (Enum.TryParse(value, true, out T result))
            {
                return result;
            }

            throw new ArgumentException(string.Format("{0} 未能找到对应的枚举.", value), "Value");
        }

    }
}