using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

#if NETSTANDARD2_1
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif

using MySql.Data.MySqlClient;
using SinGooCMS.Ado.Interface;
using Oracle.ManagedDataAccess.Client;
using System.Linq.Expressions;

namespace SinGooCMS.Ado.DbAccess
{
    public class DbAccessBase : IDbAccess
    {
        /// <summary>
        /// 数据库类型 SqlServer、MySql、Oracle、Sqlite
        /// </summary>
        protected readonly DbProviderType dbProviderType;
        /// <summary>
        /// 连接字符串
        /// </summary>
        protected readonly string connStr;
        /// <summary>
        /// 数据库版本号
        /// </summary>
        protected readonly int dbVersionNo;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="_customConnStr">连接字符串</param>
        /// <param name="_dbProviderType">数据库类型，默认是sqlserver</param>
        /// <param name="_dbVersionNo">版本号，0表示不区分</param>
        public DbAccessBase(string _customConnStr, DbProviderType _dbProviderType = DbProviderType.SqlServer, int _dbVersionNo = 0)
        {
            this.connStr = _customConnStr;
            this.dbProviderType = _dbProviderType;
            this.dbVersionNo = _dbVersionNo;
        }

        #region 参数

        public virtual DbParameter MakeParam(string parameterName, object value, ParameterDirection parameterDirection = ParameterDirection.Input)
        {
            switch (dbProviderType)
            {
                case DbProviderType.Sqlite:
                    return new SQLiteParameter(parameterName, value) { Direction = parameterDirection };
                case DbProviderType.MySql:
                    return new MySqlParameter(parameterName, value) { Direction = parameterDirection };
                case DbProviderType.Oracle:
                    return new OracleParameter(parameterName, value) { Direction = parameterDirection };
                case DbProviderType.SqlServer:
                default:
                    return new SqlParameter(parameterName, value) { Direction = parameterDirection };
            }
        }

        #endregion

        #region 执行SQL语句

        public virtual bool ExecSQL(string sql, DbParameter[] parameters = null)
        {
            using (var conn = PrepareConn())
            {
                using (var cmd = PrepareCommand(conn, sql, parameters, CommandType.Text))
                {
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public virtual async Task<bool> ExecSQLAsync(string sql, DbParameter[] parameters = null)
        {
            using (var conn = PrepareConn())
            {
                using (var cmd = PrepareCommand(conn, sql, parameters, CommandType.Text))
                {
                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
        }

        #endregion

        #region CRUD

        #region 查询

        public virtual IDataReader GetDataReader(string sql, DbParameter[] parameters = null)
        {
            var conn = PrepareConn();
            using (var cmd = PrepareCommand(conn, sql, parameters, CommandType.Text))
            {
                return cmd.ExecuteReader(CommandBehavior.CloseConnection); //调用方关掉后才会关闭连接
            }
        }

        public virtual async Task<IDataReader> GetDataReaderAsync(string sql, DbParameter[] parameters = null)
        {
            var conn = PrepareConn();
            using (var cmd = PrepareCommand(conn, sql, parameters, CommandType.Text))
            {
                return await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection); //调用方关掉后才会关闭连接
            }
        }

        public virtual DataTable GetDataTable(string sql, DbParameter[] parameters = null)
        {
            DataSet ds = GetDataSet(sql, parameters);
            return (ds != null && ds.Tables.Count > 0) ? ds.Tables[0] : null;
        }

        public virtual DataSet GetDataSet(string sql, DbParameter[] parameters = null)
        {
            using (var conn = PrepareConn())
            {
                using (var cmd = PrepareCommand(conn, sql, parameters, CommandType.Text))
                {
                    using (var da = PrepareAdapter())
                    {
                        da.SelectCommand = cmd;
                        DataSet ds = new DataSet();
                        da.Fill(ds);

                        return ds;
                    }
                }
            }
        }

        public virtual T GetValue<T>(string sql, DbParameter[] parameters = null)
        {
            var objTemp = GetObject(sql, parameters);
            return (null == objTemp || DBNull.Value == objTemp) ? default(T) : (T)Convert.ChangeType(objTemp, typeof(T));
        }
        public virtual async Task<T> GetValueAsync<T>(string sql, DbParameter[] parameters = null)
        {
            var objTemp = await GetObjectAsync(sql, parameters);
            return (null == objTemp || DBNull.Value == objTemp) ? default(T) : (T)Convert.ChangeType(objTemp, typeof(T));
        }

        public virtual IEnumerable<T> GetValueList<T>(string sql, DbParameter[] parameters = null)
        {
            var reader = GetDataReader(sql, parameters);
            var lst = new List<T>();
            while (reader.Read())
            {
                lst.Add((T)Convert.ChangeType(reader[0], typeof(T)));
            }

            reader.Close();
            return lst;
        }
        public virtual async Task<IEnumerable<T>> GetValueListAsync<T>(string sql, DbParameter[] parameters = null)
        {
            var reader = await GetDataReaderAsync(sql, parameters);
            var lst = new List<T>();
            while (reader.Read())
            {
                lst.Add((T)Convert.ChangeType(reader[0], typeof(T)));
            }

            reader.Close();
            return lst;
        }

        public virtual T GetModel<T>(IDataReader reader) where T : class
        {
            var model = default(T);
            var builder = ReflectionBuilder<T>.CreateBuilder(reader, dbProviderType);
            while (reader.Read())
            {
                model = builder.Build(reader, dbProviderType);
                break; //只取一个
            }

            reader.Close();
            return model;
        }

        public virtual T GetModel<T>(string sql, DbParameter[] parameters = null) where T : class =>
            GetModel<T>(GetDataReader(sql, parameters));

        public virtual async Task<T> GetModelAsync<T>(string sql, DbParameter[] parameters = null) where T : class =>
            GetModel<T>(await GetDataReaderAsync(sql, parameters));

        public virtual T Find<T>(object keyValue) where T : class => null;
        public virtual async Task<T> FindAsync<T>(object keyValue) where T : class => null;

        public virtual IEnumerable<T> GetList<T>(string sql, DbParameter[] parameters = null) where T : class
        {
            var lstResult = new List<T>();
            var reader = GetDataReader(sql, parameters);

            var builder = ReflectionBuilder<T>.CreateBuilder(reader, dbProviderType);
            while (reader.Read())
            {
                T tItem = builder.Build(reader, dbProviderType);
                lstResult.Add(tItem);
            }

            reader.Close();
            return lstResult;
        }
        public virtual async Task<IEnumerable<T>> GetListAsync<T>(string sql, DbParameter[] parameters = null) where T : class
        {
            var lstResult = new List<T>();
            var reader = await GetDataReaderAsync(sql, parameters);

            var builder = ReflectionBuilder<T>.CreateBuilder(reader, dbProviderType);
            while (reader.Read())
            {
                T tItem = builder.Build(reader, dbProviderType);
                lstResult.Add(tItem);
            }

            reader.Close();
            return lstResult;
        }

        public virtual IEnumerable<T> GetList<T>(int topNum = 0, string condition = "", string sort = "", string filter = "*", DbParameter[] conditionParameters = null) where T : class
            => null;
        public virtual async Task<IEnumerable<T>> GetListAsync<T>(int topNum = 0, string condition = "", string sort = "", string filter = "*", DbParameter[] conditionParameters = null) where T : class
            => null;

        #region 分页

        public virtual int GetCount(string tableName, string condition = "", DbParameter[] conditionParameters = null)
        {
            return condition.IsNullOrEmpty()
                ? GetValue<int>($" select count(*) from {tableName} ")
                : GetValue<int>($" select count(*) from {tableName} where {condition} ", conditionParameters);
        }
        public virtual async Task<int> GetCountAsync(string tableName, string condition = "", DbParameter[] conditionParameters = null)
        {
            return condition.IsNullOrEmpty()
                ? await GetValueAsync<int>($" select count(*) from {tableName} ")
                : await GetValueAsync<int>($" select count(*) from {tableName} where {condition} ", conditionParameters);
        }

        public virtual int GetCount<T>(string condition = "", DbParameter[] conditionParameters = null) =>
            GetCount(AttrAssistant.GetTableName(typeof(T)), condition);
        public virtual async Task<int> GetCountAsync<T>(string condition = "", DbParameter[] conditionParameters = null) =>
            await GetCountAsync(AttrAssistant.GetTableName(typeof(T)), condition);

        public virtual DataTable GetPagerDT(string tableName, string condition, string sort, int pageIndex, int pageSize, string filter = "*", DbParameter[] conditionParameters = null)
        => null;

        public virtual IEnumerable<T> GetPagerList<T>(string condition, string sort, int pageIndex, int pageSize, string filter = "*", DbParameter[] conditionParameters = null) where T : class
        => null;
        public virtual async Task<IEnumerable<T>> GetPagerListAsync<T>(string condition, string sort, int pageIndex, int pageSize, string filter = "*", DbParameter[] conditionParameters = null) where T : class
        => null;

        #endregion

        #endregion

        #region 插入

        public virtual int InsertModel<T>(T model, string tableName = "") where T : class
        => 0;
        public virtual async Task<int> InsertModelAsync<T>(T model, string tableName = "") where T : class
        => 0;

        #endregion

        #region 更新

        public virtual bool UpdateModel<T>(T model) where T : class
        {
            var arrProperty = typeof(T).GetProperties();
            var builderSQL = new StringBuilder();
            var lstParams = new List<DbParameter>();

            foreach (var property in arrProperty)
            {
                //关键字key 和 不属于表的属性NotMapped 不更新
                if (AttrAssistant.IsKey(property) || AttrAssistant.IsNotMapped(property))
                    continue;

                //没有提供数据的字段也不处理
                object obj = property.GetValue(model, null);
                if (obj == null)
                    continue;

                builderSQL.AppendFormat("{0}=@{0} , ", property.Name);
                lstParams.Add(MakeParam("@" + property.Name, obj));
            }

            builderSQL.Remove(builderSQL.Length - 2, 2);
            string key = AttrAssistant.GetKey(typeof(T));
            if (!key.IsNullOrEmpty())
            {
                //主键值
                object primaryKeyUniqueValue = RefProperty.GetPropertyValue(model, key);
                builderSQL.AppendFormat(" where {0}=@primaryKeyUniqueValue ", key);
                lstParams.Add(MakeParam("@primaryKeyUniqueValue", primaryKeyUniqueValue));
            }

            return ExecSQL(" update " + AttrAssistant.GetTableName(typeof(T)) + " set " + builderSQL.ToString(), lstParams.ToArray());
        }

        public virtual async Task<bool> UpdateModelAsync<T>(T model) where T : class
        {
            var arrProperty = typeof(T).GetProperties();
            var builderSQL = new StringBuilder();
            var lstParams = new List<DbParameter>();

            foreach (var property in arrProperty)
            {
                //关键字key 和 不属于表的属性NotMapped 不更新
                if (AttrAssistant.IsKey(property) || AttrAssistant.IsNotMapped(property))
                    continue;

                //没有提供数据的字段也不处理
                object obj = property.GetValue(model, null);
                if (obj == null)
                    continue;

                builderSQL.AppendFormat("{0}=@{0} , ", property.Name);
                lstParams.Add(MakeParam("@" + property.Name, obj));
            }

            builderSQL.Remove(builderSQL.Length - 2, 2);
            string key = AttrAssistant.GetKey(typeof(T));
            if (!key.IsNullOrEmpty())
            {
                //主键值
                object primaryKeyUniqueValue = RefProperty.GetPropertyValue(model, key);
                builderSQL.AppendFormat(" where {0}=@primaryKeyUniqueValue ", key);
                lstParams.Add(MakeParam("@primaryKeyUniqueValue", primaryKeyUniqueValue));
            }

            return await ExecSQLAsync(" update " + AttrAssistant.GetTableName(typeof(T)) + " set " + builderSQL.ToString(), lstParams.ToArray());
        }

        public virtual bool UpdateColumn<T>(Expression<Func<T, T>> columns, string condition = "", DbParameter[] conditionParameters = null) where T : class
        {
            var builder = new StringBuilder();
            var parameters = new List<DbParameter>();
            builder.Append($" update {AttrAssistant.GetTableName(typeof(T))} set ");

            var bindings = (columns.Body as MemberInitExpression).Bindings;
            foreach (var item in bindings)
            {
                builder.AppendFormat("{0}=@{0},", item.Member.Name);
                parameters.Add(MakeParam("@" + item.Member.Name, ((item as MemberAssignment).Expression as ConstantExpression).Value));
            }

            string sql = builder.ToString().TrimEnd(',');
            if (!condition.IsNullOrEmpty())
            {
                sql += " where " + condition;
                if (conditionParameters != null)
                    parameters.AddRange(conditionParameters);
            }

            return ExecSQL(sql, parameters.ToArray());
        }

        public virtual async Task<bool> UpdateColumnAsync<T>(Expression<Func<T, T>> columns, string condition = "", DbParameter[] conditionParameters = null) where T : class
        {
            var builder = new StringBuilder();
            var parameters = new List<DbParameter>();
            builder.Append($" update {AttrAssistant.GetTableName(typeof(T))} set ");

            var bindings = (columns.Body as MemberInitExpression).Bindings;
            foreach (var item in bindings)
            {
                builder.AppendFormat("{0}=@{0},", item.Member.Name);
                parameters.Add(MakeParam("@" + item.Member.Name, ((item as MemberAssignment).Expression as ConstantExpression).Value));
            }

            string sql = builder.ToString().TrimEnd(',');
            if (!condition.IsNullOrEmpty())
            {
                sql += " where " + condition;
                if (conditionParameters != null)
                    parameters.AddRange(conditionParameters);
            }

            return await ExecSQLAsync(sql, parameters.ToArray());
        }

        #endregion

        #region 删除

        public virtual bool DeleteModel<T>(T model) where T : class
        {
            var type = typeof(T);
            string key = AttrAssistant.GetKey(type);
            if (!key.IsNullOrEmpty())
            {
                object primaryKeyUniqueValue = RefProperty.GetPropertyValue(model, key);
                return Delete<T>(primaryKeyUniqueValue);
            }

            return false;
        }
        public virtual async Task<bool> DeleteModelAsync<T>(T model) where T : class
        {
            var type = typeof(T);
            string key = AttrAssistant.GetKey(type);
            if (!key.IsNullOrEmpty())
            {
                object primaryKeyUniqueValue = RefProperty.GetPropertyValue(model, key);
                return await DeleteAsync<T>(primaryKeyUniqueValue);
            }

            return false;
        }

        public virtual bool Delete<T>(object keyValue)
        {
            if (keyValue.IsNullOrEmpty())
                throw new ArgumentException("参数 keyValue 值不能为空");

            var type = typeof(T);
            string tableName = AttrAssistant.GetTableName(type);
            string key = AttrAssistant.GetKey(type);

            string sql = $" delete from {tableName} where {key}=@{key} ";
            var parameters = new DbParameter[]{
                MakeParam($"@{key}", keyValue)
            };

            return ExecSQL(sql, parameters);
        }
        public virtual async Task<bool> DeleteAsync<T>(object keyValue)
        {
            if (keyValue.IsNullOrEmpty())
                throw new ArgumentException("参数 keyValue 值不能为空");

            var type = typeof(T);
            string tableName = AttrAssistant.GetTableName(type);
            string key = AttrAssistant.GetKey(type);

            string sql = $" delete from {tableName} where {key}=@{key} ";
            var parameters = new DbParameter[]{
                MakeParam($"@{key}", keyValue)
            };

            return await ExecSQLAsync(sql, parameters);
        }

        #endregion

        #endregion        

        #region 事务

        public virtual void ExecTrans(IEnumerable<string> lstSql)
        {
            using (var conn = PrepareConn())
            {
                conn.Open();
                //启动一个事务
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = transaction;
                            foreach (var item in lstSql)
                            {
                                cmd.CommandText = item;
                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit(); //执行
                        }
                    }
                    catch
                    {
                        transaction.Rollback(); //回滚
                    }
                }

            }
        }

        public virtual async Task ExecTransAsync(IEnumerable<string> lstSql)
        {
            using (var conn = PrepareConn())
            {
                conn.Open();
                //启动一个事务
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = transaction;
                            foreach (var item in lstSql)
                            {
                                cmd.CommandText = item;
                                cmd.ExecuteNonQuery();
                            }
#if NETSTANDARD2_1
                            await transaction.CommitAsync(); //执行
#else
                            transaction.Commit(); //执行
#endif
                        }
                    }
                    catch
                    {
#if NETSTANDARD2_1
                        await transaction.RollbackAsync(); //回滚
#else
                        transaction.Rollback(); //回滚
#endif
                    }
                }

            }
        }

        public virtual void ExecTrans(IDictionary<string, DbParameter[]> lstSql)
        {
            using (var conn = PrepareConn())
            {
                conn.Open();
                //启动一个事务
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = transaction;
                            foreach (var item in lstSql)
                            {
                                cmd.CommandText = item.Key;
                                if (item.Value != null)
                                    cmd.Parameters.AddRange(item.Value); //添加参数

                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit(); //执行
                        }
                    }
                    catch
                    {
                        transaction.Rollback(); //回滚
                    }
                }

            }
        }

        public virtual async Task ExecTransAsync(IDictionary<string, DbParameter[]> lstSql)
        {
            using (var conn = PrepareConn())
            {
                conn.Open();
                //启动一个事务
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = transaction;
                            foreach (var item in lstSql)
                            {
                                cmd.CommandText = item.Key;
                                if (item.Value != null)
                                    cmd.Parameters.AddRange(item.Value); //添加参数

                                cmd.ExecuteNonQuery();
                            }
#if NETSTANDARD2_1
                            await transaction.CommitAsync(); //执行
#else
                            transaction.Commit(); //执行
#endif
                        }
                    }
                    catch
                    {
#if NETSTANDARD2_1
                        await transaction.RollbackAsync(); //回滚
#else
                        transaction.Rollback(); //回滚
#endif
                    }
                }

            }
        }

        #endregion

        #region 存储过程

        public bool ExecProc(string commandText, DbParameter[] arrParams)
        {
            using (var conn = PrepareConn())
            {
                using (var cmd = PrepareCommand(conn, commandText, arrParams, CommandType.StoredProcedure))
                {
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public object ExecProcReValue(string commandText, DbParameter[] arrParams)
        {
            using (var conn = PrepareConn())
            {
                using (var cmd = PrepareCommand(conn, commandText, arrParams, CommandType.StoredProcedure))
                {
                    return cmd.ExecuteScalar();
                }
            }
        }

        public IDataReader ExecProcReReader(string commandText, DbParameter[] arrParams)
        {
            var conn = PrepareConn();
            using (var cmd = PrepareCommand(conn, commandText, arrParams, CommandType.StoredProcedure))
            {
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
        }

        public DataSet ExecProcReDS(string commandText, DbParameter[] arrParams)
        {
            using (var conn = PrepareConn())
            {
                using (var cmd = PrepareCommand(conn, commandText, arrParams, CommandType.StoredProcedure))
                {
                    using (var da = PrepareAdapter())
                    {
                        da.SelectCommand = cmd;
                        DataSet ds = new DataSet();
                        da.Fill(ds);

                        return ds;
                    }
                }
            }
        }

        public DataTable ExecProcReDT(string commandText, DbParameter[] arrParams)
        {
            var ds = ExecProcReDS(commandText, arrParams);
            return (ds != null && ds.Tables.Count > 0) ? ds.Tables[0] : null;
        }

        #endregion

        #region Helper

        /// <summary>
        /// 读取对象
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected object GetObject(string sql, DbParameter[] parameters = null)
        {
            using (var conn = PrepareConn())
            {
                using (var cmd = PrepareCommand(conn, sql, parameters, CommandType.Text))
                {
                    return cmd.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// 读取对象
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected async Task<object> GetObjectAsync(string sql, DbParameter[] parameters = null)
        {
            using (var conn = PrepareConn())
            {
                using (var cmd = PrepareCommand(conn, sql, parameters, CommandType.Text))
                {
                    return await cmd.ExecuteScalarAsync();
                }
            }
        }

        /// <summary>
        /// 初始化命令
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        protected DbCommand PrepareCommand(DbConnection conn, string sql, DbParameter[] parameters = null, CommandType commandType = CommandType.Text)
        {
            DbCommand cmd;
            switch (dbProviderType)
            {
                case DbProviderType.Sqlite:
                    cmd = new SQLiteCommand();
                    break;
                case DbProviderType.MySql:
                    cmd = new MySqlCommand();
                    break;
                case DbProviderType.Oracle:
                    cmd = new OracleCommand();
                    break;
                case DbProviderType.SqlServer:
                default:
                    cmd = new SqlCommand();
                    break;
            }

            conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = sql;
            cmd.CommandType = commandType;
            if (parameters != null)
                cmd.Parameters.AddRange(parameters); //添加参数，如果有

            return cmd;
        }

        /// <summary>
        /// 初始化连接
        /// </summary>
        /// <returns></returns>
        protected DbConnection PrepareConn()
        {
            switch (dbProviderType)
            {
                case DbProviderType.Sqlite:
                    return new SQLiteConnection(connStr);
                case DbProviderType.MySql:
                    return new MySqlConnection(connStr);
                case DbProviderType.Oracle:
                    return new OracleConnection(connStr);
                case DbProviderType.SqlServer:
                default:
                    return new SqlConnection(connStr);
            }
        }

        /// <summary>
        /// 初始化适配器
        /// </summary>
        /// <returns></returns>
        protected DbDataAdapter PrepareAdapter()
        {
            switch (dbProviderType)
            {
                case DbProviderType.Sqlite:
                    return new SQLiteDataAdapter();
                case DbProviderType.MySql:
                    return new MySqlDataAdapter();
                case DbProviderType.Oracle:
                    return new OracleDataAdapter();
                case DbProviderType.SqlServer:
                default:
                    return new SqlDataAdapter();
            }
        }

        #endregion
    }
}
