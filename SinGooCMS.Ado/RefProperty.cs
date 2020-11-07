using System;
using System.Reflection;
using System.Linq;

namespace SinGooCMS.Ado
{
    /// <summary>
    /// 读取属性值
    /// </summary>
    internal class RefProperty
    {
        public static decimal GetSafePropertyDecimal<T>(T model, string strFindName)
        {
            string strResult = GetSafePropertyString<T>(model, strFindName);
            if (decimal.TryParse(strResult, out decimal decReturn))
                return decReturn;

            return 0.0m;
        }

        public static int GetSafePropertyInt32<T>(T model, string strFindName)
        {
            string strResult = GetSafePropertyString<T>(model, strFindName);
            if (int.TryParse(strResult, out int intReturn))
                return intReturn;

            return 0;
        }

        public static DateTime GetSafePropertyDateTime<T>(T model, string strFindName)
        {
            string strResult = GetSafePropertyString<T>(model, strFindName);
            if (DateTime.TryParse(strResult, out DateTime dtReturn))
                return dtReturn;

            return new DateTime(1900, 1, 1);
        }

        public static string GetSafePropertyString<T>(T model, string strFindName) =>
            GetPropertyValue<T>(model, strFindName).ToString();

        public static object GetPropertyValue<T>(T model, string strFindName)
        {
            PropertyInfo[] arrProperty = typeof(T).GetProperties();
            var pi = arrProperty.Where(p => p.Name.Equals(strFindName)).FirstOrDefault();
            return (pi?.GetValue(model, null)) ?? string.Empty;
        }
    }
}
