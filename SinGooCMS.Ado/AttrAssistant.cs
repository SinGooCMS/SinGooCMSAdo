using System;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SinGooCMS.Ado
{
    internal class AttrAssistant
    {
        /// <summary>
        /// 读取数据库表名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetTableName(Type type)
        {
            var tableAttr = type.GetCustomAttributes(false).Where(attr => attr.GetType().Name == "TableAttribute").SingleOrDefault() as dynamic;
            return (tableAttr?.Name) ?? type.Name; //没有定义表名特性，那么表名取类名
        }

        /// <summary>
        /// 读取主键 - 只支持单主键
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetKey(Type type)
        {
            var pi = type.GetProperties().Where(p => p.GetCustomAttributes(true).Any(a => a is KeyAttribute)).FirstOrDefault();

            var key = pi?.Name ?? "";
            if (key.IsNullOrEmpty())
                throw new ArgumentException($"找不到类型【{type.FullName}】的主键名称，请检查是否有设置KeyAttribute");

            return key;
        }

        /// <summary>
        /// 读取主键值 - 只支持单主键
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static object GetKeyValue<TEntity>(TEntity entity) where TEntity : class
        {
            var pi = typeof(TEntity).GetProperties().Where(p => p.GetCustomAttributes(true).Any(a => a is KeyAttribute)).FirstOrDefault();
            return pi?.GetValue(entity, null);
        }

        /// <summary>
        /// 列是否主键 - 只支持单主键
        /// </summary>
        /// <param name="pi"></param>
        /// <returns></returns>
        public static bool IsKey(PropertyInfo pi) =>
            pi.GetCustomAttributes(true).Any(a => a is KeyAttribute);

        /// <summary>
        /// 列是否映射(自定义不属于表的字段需要NotMapped属性)
        /// </summary>
        /// <param name="pi"></param>
        /// <returns></returns>
        public static bool IsNotMapped(PropertyInfo pi) =>
            pi.GetCustomAttributes(true).Any(a => a is NotMappedAttribute);
    }
}
