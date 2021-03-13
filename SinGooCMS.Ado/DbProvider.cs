using System;
using System.Text;
using System.IO;
using System.Reflection;
using SinGooCMS.Ado.Interface;

namespace SinGooCMS.Ado
{
    /// <summary>
    /// 提供具体数据库操作对象
    /// </summary>
    public class DbProvider
    {
        #region DDL

        /// <summary>
        /// 创建表维护实例
        /// </summary>
        /// <param name="dbProviderType">数据库类型，对应SinGooCMS.DbType</param>
        /// <param name="customConnStr">自定义数据库连接</param>
        /// <returns></returns>
        public static IDbMaintenance CreateDbMaintenance(DbProviderType dbProviderType, string customConnStr = "")
        {
            Assembly tempAssembly = Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SinGooCMS.Ado.dll"));
            Type type = tempAssembly.GetType($"SinGooCMS.Ado.DbMaintenance.{dbProviderType}DbMaintenance");
            return !customConnStr.IsNullOrEmpty()
                ? (IDbMaintenance)(Activator.CreateInstance(type, new string[] { customConnStr }))
                : (IDbMaintenance)(Activator.CreateInstance(type));
        }

        private static IDbMaintenance _dbMaintenance = null;
        /// <summary>
        /// DDL对象
        /// </summary>
        public static IDbMaintenance DbMaintenance
        {
            get
            {
                if (_dbMaintenance != null)
                    return _dbMaintenance;
                else
                {
                    _dbMaintenance = CreateDbMaintenance(Utils.ProviderType);
                    return _dbMaintenance;
                }
            }
        }

        #endregion

        #region DML        

        /// <summary>
        /// 创建Ado实例
        /// </summary>
        /// <param name="options">选项</param>
        /// <returns></returns>
        public static IDbAccess Create(DbAccessOptions options)
        {
            return Create(options.DbProviderType, options.ConnectionString, options.DbVersionNo);
        }

        private static IDbAccess _dbAccess = null;
        /// <summary>
        /// DML对象
        /// </summary>
        public static IDbAccess DbAccess
        {
            get
            {
                if (_dbAccess != null)
                    return _dbAccess;
                else
                {
                    _dbAccess = Create(Utils.ProviderType);
                    return _dbAccess;
                }
            }
        }

        /// <summary>
        /// 创建Ado实例
        /// </summary>
        /// <param name="dbProviderType">数据库类型，对应SinGooCMS.DbProviderType</param>
        /// <param name="customConnStr">数据库连接字符串</param>
        /// <param name="dbVersionNo">数据库版本号</param>
        /// <returns></returns>
        public static IDbAccess Create(DbProviderType dbProviderType, string customConnStr = "", int dbVersionNo = 0)
        {
            Assembly tempAssembly = Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SinGooCMS.Ado.dll"));
            Type type = tempAssembly.GetType($"SinGooCMS.Ado.DbAccess.{dbProviderType}Access");
            return !customConnStr.IsNullOrEmpty()
                ? (IDbAccess)(Activator.CreateInstance(type, new object[] { customConnStr, dbVersionNo }))
                : (IDbAccess)(Activator.CreateInstance(type)); //默认的连接
        }

        #endregion
    }
}
