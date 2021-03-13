using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

#if NETSTANDARD2_1
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
#endif

using System.Data;

namespace SinGooCMS.Ado
{
    internal class Utils
    {
        //分页表最终别名
        public readonly static string SinGooPagerAlias = "SinGooPagerAlias";

#if NETSTANDARD2_1
        public static IConfiguration Configuration { get; set; }
        static Utils()
        {
            Configuration = new ConfigurationBuilder()
            .Add(new JsonConfigurationSource { Path = "appsettings.json", ReloadOnChange = true })
            .Build();
        }
#endif

        /// <summary>
        /// 获取数据库枚举类型
        /// </summary>
        public static DbProviderType ProviderType
        {
            get
            {
                try
                {
                    return (DbProviderType)Enum.Parse(typeof(DbProviderType), ProviderName);
                }
                catch
                {
                    throw new Exception("ProviderName与枚举类型不匹配");
                }
            }
        }

        /// <summary>
        /// 数据库类型 必须按 SinGooCMS.DbType 的枚举类型
        /// </summary>
        public static string ProviderName
        {
            get
            {
#if NETSTANDARD2_1
                try
                {
                    return Configuration.GetConnectionString("ProviderName");
                }
                catch
                {
                    throw new FileNotFoundException("文件appsettings.json连接字符串是否缺失ProviderName配置");
                }
#else
                try
                {
                    return System.Configuration.ConfigurationManager.ConnectionStrings["SQLConnSTR"].ProviderName;
                }
                catch
                {
                    throw new FileNotFoundException("连接字符串[name='SQLConnSTR']是否缺失ProviderName配置");
                }
#endif
            }
        }
        /// <summary>
        /// 连接库连接字符串
        /// </summary>
        public static string DefConnStr
        {
            get
            {
#if NETSTANDARD2_1
                try
                {
                    return Configuration.GetConnectionString("SQLConnSTR");
                }
                catch
                {
                    throw new FileNotFoundException("文件appsettings.json连接字符串是否缺失SQLConnSTR配置");
                }
#else
                try
                {
                    return System.Configuration.ConfigurationManager.ConnectionStrings["SQLConnSTR"].ConnectionString;
                }
                catch
                {
                    throw new FileNotFoundException("连接字符串[name='SQLConnSTR']是否缺失ConnectionString配置");
                }
#endif
            }
        }

        /// <summary>
        /// 数据库版本号
        /// </summary>
        public static int DbVersionNo
        {
            get
            {
#if NETSTANDARD2_1
                return Configuration.GetSection("AppSettings")["DbVersionNo"]?.ToInt() ?? 0;
#else
                return System.Configuration.ConfigurationManager.AppSettings["DbVersionNo"]?.ToInt() ?? 0;
#endif
            }
        }

        public static bool IsCharColumn(string dataType) =>
             (new string[] { "char", "varchar", "nvarchar" }).Contains(dataType.ToLower());

        public static string GetMapPath(string strPath)
        {
            strPath = strPath.Replace("~/", "/").Replace("\\", "/");
            if (strPath.StartsWith("/"))
                strPath = strPath.TrimStart('/');

#if NETSTANDARD2_1
            return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/"), strPath);
#else
            if (null != System.Web.HttpContext.Current)
                return System.Web.HttpContext.Current.Server.MapPath(strPath);
            else
                return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/"), strPath);
#endif
        }
    }

    internal static class StringExtension
    {
        /// <summary>
        /// 是否为null或者空串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this object obj)
        {
            return obj == null || obj.ToString().Trim().Length == 0;
        }
    }

    internal static class DataChangeExtension
    {
        #region 数据类型转换

        /// <summary>
        /// 转化为 byte 整形(0 到 255)
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="defValue"></param>
        /// <returns></returns>
        public static byte ToByte(this object thisValue, byte defValue = 0)
        {
            if (thisValue != null)
            {
                if (byte.TryParse(thisValue.ToString(), out byte result))
                    return result;
            }

            return defValue;
        }

        /// <summary>
        /// 转化为短整型
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="defValue"></param>
        /// <returns></returns>
        public static short ToShort(this object thisValue, short defValue = 0)
        {
            if (thisValue != null)
            {
                if (Int16.TryParse(thisValue.ToString(), out short result))
                    return result;
            }

            return defValue;
        }

        /// <summary>
        /// 转化为整型
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="defValue"></param>
        /// <returns></returns>
        public static int ToInt(this object thisValue, int defValue = 0)
        {
            if (thisValue != null)
            {
                if (Int32.TryParse(thisValue.ToString(), out int result))
                    return result;
            }

            return defValue;
        }

        /// <summary>
        /// 转化为长整型
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="defValue"></param>
        /// <returns></returns>
        public static long ToLong(this object thisValue, long defValue = 0)
        {
            if (thisValue != null)
            {
                if (Int64.TryParse(thisValue.ToString(), out long result))
                    return result;
            }

            return defValue;
        }

        /// <summary>
        /// 转化为布尔型
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="defValue"></param>
        /// <returns></returns>
        public static bool ToBool(this object thisValue, bool defValue = false)
        {
            if (thisValue != null)
            {
                if (string.Compare(thisValue.ToString(), "true", true).Equals(0))
                    return true;

                if (thisValue.ToInt() == 1) //sqlite没有布尔型
                    return true;

                if (Boolean.TryParse(thisValue.ToString(), out bool result))
                    return result;
            }

            return defValue;
        }

        /// <summary>
        /// 转化为浮点型
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="defValue"></param>
        /// <returns></returns>
        public static float ToFloat(this object thisValue, float defValue = 0.0f)
        {
            if (thisValue != null)
            {
                if (float.TryParse(thisValue.ToString(), out float result))
                    return result;
            }

            return defValue;
        }

        /// <summary>
        /// 转化为双精度数据类型
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="defValue"></param>
        /// <returns></returns>
        public static double ToDouble(this object thisValue, double defValue = 0.0d)
        {
            if (thisValue != null)
            {
                if (Double.TryParse(thisValue.ToString(), out double result))
                    return result;
            }

            return defValue;
        }

        /// <summary>
        /// 转化为数据类型
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="defValue"></param>
        /// <returns></returns>
        public static decimal ToDecimal(this object thisValue, decimal defValue = 0.0m)
        {
            if (thisValue != null)
            {
                if (decimal.TryParse(thisValue.ToString(), out decimal result))
                    return result;
            }

            return defValue;
        }

        /// <summary>
        /// 转化为日期类型
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this object thisValue) =>
            thisValue.ToDateTime(new DateTime(1900, 1, 1));

        /// <summary>
        /// 转化为日期类型
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="defValue"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this object thisValue, DateTime defValue)
        {
            if (thisValue != null)
            {
                if (DateTime.TryParse(thisValue.ToString(), out DateTime result))
                    return result;
            }

            return defValue;
        }

        /// <summary>
        /// ids字符串转整型数组
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static int[] ToIntArray(this string ids)
        {
            List<int> result = new List<int>();
            string[] arr = ids.Split(',');
            foreach (var item in arr)
            {
                if (int.TryParse(item, out int outValue))
                {
                    result.Add(outValue);
                }
            }

            return result.ToArray();
        }

        #endregion
    }

    /// <summary>
    /// 数据库类型
    /// </summary>
    public enum DbProviderType
    {
        SqlServer,
        MySql,
        Sqlite,
        Oracle
    }
}
