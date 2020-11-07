using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Data;
using System.Linq;

namespace SinGooCMS.Ado
{
    /// <summary>
    /// 通过datareader反射出实体并填充值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ReflectionBuilder<T>
    {
        private PropertyInfo[] properties;
        private ReflectionBuilder() { }

        /// <summary>
        /// 赋值给实体,并返回实体
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="dbProviderType"></param>
        /// <returns></returns>
        public T Build(IDataReader reader, DbProviderType dbProviderType = DbProviderType.SqlServer)
        {
            T result = (T)Activator.CreateInstance(typeof(T));

            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (properties[i] != null && !reader.IsDBNull(i))
                {
                    if (dbProviderType == DbProviderType.SqlServer)
                        properties[i].SetValue(result, reader[i], null);
                    else
                    {
                        //sqlserver以外的数据库要进行类型转换，不同数据库类型各异，但都要转化成实体类属性的类型
                        string fullName = properties[i].PropertyType.FullName;
                        if (fullName.IndexOf("System.Byte") != -1)
                            properties[i].SetValue(result, reader[i].ToByte(), null);
                        else if (fullName.IndexOf("System.Int16") != -1)
                            properties[i].SetValue(result, reader[i].ToShort(), null);
                        else if (fullName.IndexOf("System.Int32") != -1)
                            properties[i].SetValue(result, reader[i].ToInt(), null);
                        else if (fullName.IndexOf("System.Int64") != -1)
                            properties[i].SetValue(result, reader[i].ToLong(), null);
                        else if (fullName.IndexOf("System.Single") != -1)
                            properties[i].SetValue(result, reader[i].ToFloat(), null);
                        else if (fullName.IndexOf("System.Double") != -1)
                            properties[i].SetValue(result, reader[i].ToDouble(), null);
                        else if (fullName.IndexOf("System.Decimal") != -1)
                            properties[i].SetValue(result, reader[i].ToDecimal(), null);
                        else if (fullName.IndexOf("System.Boolean") != -1)
                            properties[i].SetValue(result, reader[i].ToBool(), null);
                        else if (fullName.IndexOf("System.DateTime") != -1)
                            properties[i].SetValue(result, reader[i].ToDateTime(), null);
                        else if (fullName.IndexOf("System.String") != -1)
                            properties[i].SetValue(result, reader[i].ToString(), null);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 创建实体
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="dbProviderType"></param>
        /// <returns></returns>
        internal static ReflectionBuilder<T> CreateBuilder(IDataReader reader, DbProviderType dbProviderType = DbProviderType.SqlServer)
        {
            ReflectionBuilder<T> result = new ReflectionBuilder<T>();

            result.properties = new PropertyInfo[reader.FieldCount];
            var allProperties = typeof(T).GetProperties();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (dbProviderType == DbProviderType.Oracle)
                {
                    //oracle的reader读出来的字段全是大写的，无法和实体类的属性直接匹配上 这是非常fuck的事
                    //所以我们要先匹配一下，不区分大小写
                    var temp = allProperties.Where(p => string.Compare(p.Name, reader.GetName(i), true) == 0).FirstOrDefault();
                    if (temp != null)
                        result.properties[i] = typeof(T).GetProperty(temp.Name);
                }
                else
                    result.properties[i] = typeof(T).GetProperty(reader.GetName(i));
            }

            return result;
        }
    }
}
