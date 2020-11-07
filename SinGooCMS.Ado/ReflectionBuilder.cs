using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Data;
using System.Linq;

namespace SinGooCMS.Ado
{
    /// <summary>
    /// ͨ��datareader�����ʵ�岢���ֵ
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ReflectionBuilder<T>
    {
        private PropertyInfo[] properties;
        private ReflectionBuilder() { }

        /// <summary>
        /// ��ֵ��ʵ��,������ʵ��
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
                        //sqlserver��������ݿ�Ҫ��������ת������ͬ���ݿ����͸��죬����Ҫת����ʵ�������Ե�����
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
        /// ����ʵ��
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
                    //oracle��reader���������ֶ�ȫ�Ǵ�д�ģ��޷���ʵ���������ֱ��ƥ���� ���Ƿǳ�fuck����
                    //��������Ҫ��ƥ��һ�£������ִ�Сд
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
