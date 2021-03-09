using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SinGooCMS.Ado.Interface;

namespace SinGooCMS.Ado.DbAccess
{
    /*
     * 1、oracle 主键没有自增列，需要创建一个序列，然后往里面填数值
     * 2、oracle 的参数是 :param 而不是 @param
     * 3、取前N条记录 使用 where rownum<=N 而不是 top N 或者 limit N
     */

    public class OracleAccess : DbAccessBase, IDbAccess, IOracle
    {
        public OracleAccess(string _customConnStr)
            : base(_customConnStr, DbProviderType.Oracle)
        {
            //
        }

        public OracleAccess() :
            this(Utils.DefConnStr)
        {
            //默认连接字符串
        }

        #region CRUD

        #region 查询        

        public override T Find<T>(object keyValue)
        {
            string tableName = AttrAssistant.GetTableName(typeof(T));
            string key = AttrAssistant.GetKey(typeof(T));

            return GetModel<T>(GetDataReader($"select * from {tableName} where {key} = :KeyValue and rownum<=1", new DbParameter[] { MakeParam(":KeyValue", keyValue) }));
        }
        public override async Task<T> FindAsync<T>(object keyValue)
        {
            string tableName = AttrAssistant.GetTableName(typeof(T));
            string key = AttrAssistant.GetKey(typeof(T));

            return GetModel<T>(await GetDataReaderAsync($"select * from {tableName} where {key} = :KeyValue and rownum<=1", new DbParameter[] { MakeParam(":KeyValue", keyValue) }));
        }

        public override IEnumerable<T> GetList<T>(int topNum = 0, string condition = "", string sort = "", string filter = "*", DbParameter[] conditionParameters = null)
        {
            //select * from (select * from table) where rownum <= 3 order by rownum asc
            string tableName = AttrAssistant.GetTableName(typeof(T));
            var builder = new StringBuilder("select * from (select ");

            if (string.IsNullOrEmpty(filter))
                filter = "*";

            builder.AppendFormat(" {0} from {1} ", filter, tableName);
            if (!string.IsNullOrEmpty(condition))
                builder.AppendFormat(" where {0} ", condition);
            if (!string.IsNullOrEmpty(sort))
                builder.AppendFormat(" order by {0} ", sort);
            builder.Append(")");

            if (topNum > 0)
                builder.AppendFormat(" where rownum<={0}", topNum);

            return GetList<T>(builder.ToString(), conditionParameters);
        }
        public override async Task<IEnumerable<T>> GetListAsync<T>(int topNum = 0, string condition = "", string sort = "", string filter = "*", DbParameter[] conditionParameters = null)
        {
            //select * from (select * from table) where rownum <= 3 order by rownum asc
            string tableName = AttrAssistant.GetTableName(typeof(T));
            var builder = new StringBuilder("select * from (select ");

            if (string.IsNullOrEmpty(filter))
                filter = "*";

            builder.AppendFormat(" {0} from {1} ", filter, tableName);
            if (!string.IsNullOrEmpty(condition))
                builder.AppendFormat(" where {0} ", condition);
            if (!string.IsNullOrEmpty(sort))
                builder.AppendFormat(" order by {0} ", sort);
            builder.Append(")");

            if (topNum > 0)
                builder.AppendFormat(" where rownum<={0}", topNum);

            return await GetListAsync<T>(builder.ToString(), conditionParameters);
        }

        #region 分页

        public override DataTable GetPagerDT(string tableName, string condition, string sort, int pageIndex, int pageSize, string filter = "*", DbParameter[] conditionParameters = null)
        {
            if (string.IsNullOrEmpty(condition))
                condition = "1=1";

            //起始记录
            int startPosition = (pageIndex - 1) * pageSize + 1;
            //截止记录
            int endPosition = pageIndex * pageSize;

            //SELECT * FROM (SELECT t.*,ROWNUM r FROM TABLE t WHERE ROWNUM <= pageNumber*pageSize) WHERE r >(pageNumber)*pageSize
            var builder = new StringBuilder();
            builder.AppendFormat(@"select {0} from
                                ( 
                                    select t.*,rownum as rowno from 
                                    (
                                        select * from {1} {2} {3}
                                    ) t where rownum<={5}
                                ) {6} where rowno>={4}",
                                filter, tableName,
                                condition.IsNullOrEmpty() ? "" : " where " + condition,
                                sort.IsNullOrEmpty() ? "" : " order by " + sort, startPosition, endPosition, Utils.SinGooPagerAlias);

            return GetDataTable(builder.ToString(), conditionParameters);
        }

        public override IEnumerable<T> GetPagerList<T>(string condition, string sort, int pageIndex, int pageSize, string filter = "*", DbParameter[] conditionParameters = null)
        {
            var lstResult = new List<T>();
            T tItem = default(T);
            string tableName = AttrAssistant.GetTableName(typeof(T)); //表名

            if (string.IsNullOrEmpty(condition))
                condition = "1=1";

            //起始记录
            int startPosition = (pageIndex - 1) * pageSize + 1;
            //截止记录
            int endPosition = pageIndex * pageSize;

            var builder = new StringBuilder();
            builder.AppendFormat(@"select {0} from
                                ( 
                                    select t.*,rownum as rowno from 
                                    (
                                        select * from {1} {2} {3}
                                    ) t where rownum<={5}
                                ) {6} where rowno>={4}",
                                filter, tableName,
                                condition.IsNullOrEmpty() ? "" : " where " + condition,
                                sort.IsNullOrEmpty() ? "" : " order by " + sort, startPosition, endPosition, Utils.SinGooPagerAlias);

            var reader = GetDataReader(builder.ToString(), conditionParameters);
            var refBuilder = ReflectionBuilder<T>.CreateBuilder(reader, dbProviderType);
            while (reader.Read())
            {
                tItem = refBuilder.Build(reader, dbProviderType);
                lstResult.Add(tItem);
            }

            reader.Close();
            return lstResult;
        }
        public override async Task<IEnumerable<T>> GetPagerListAsync<T>(string condition, string sort, int pageIndex, int pageSize, string filter = "*", DbParameter[] conditionParameters = null)
        {
            var lstResult = new List<T>();
            T tItem = default(T);
            string tableName = AttrAssistant.GetTableName(typeof(T)); //表名

            if (string.IsNullOrEmpty(condition))
                condition = "1=1";

            //起始记录
            int startPosition = (pageIndex - 1) * pageSize + 1;
            //截止记录
            int endPosition = pageIndex * pageSize;

            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(@"select {0} from
                                ( 
                                    select t.*,rownum as rowno from 
                                    (
                                        select * from {1} {2} {3}
                                    ) t where rownum<={5}
                                ) {6} where rowno>={4}",
                                filter, tableName,
                                condition.IsNullOrEmpty() ? "" : " where " + condition,
                                sort.IsNullOrEmpty() ? "" : " order by " + sort, startPosition, endPosition, Utils.SinGooPagerAlias);

            var reader = await GetDataReaderAsync(builder.ToString(), conditionParameters);
            var refBuilder = ReflectionBuilder<T>.CreateBuilder(reader, dbProviderType);
            while (reader.Read())
            {
                tItem = refBuilder.Build(reader, dbProviderType);
                lstResult.Add(tItem);
            }

            reader.Close();
            return lstResult;
        }

        #endregion

        #endregion

        #region 插入

        public override int InsertModel<T>(T model, string tableName = "")
        {
            var dictSql = new Dictionary<string, (string, DbParameter[])>();

            var arrProperty = typeof(T).GetProperties();
            var builderSQL = new StringBuilder();
            var builderParams = new StringBuilder(" ( ");
            var lstParams = new List<DbParameter>();
            bool isKeyAutoSEQ = false;

            if (tableName.IsNullOrEmpty())
                tableName = AttrAssistant.GetTableName(typeof(T));

            foreach (PropertyInfo property in arrProperty)
            {
                //oracle没有自动增长的列，创建了序列也要主动赋值
                if (AttrAssistant.IsKey(property) && property.PropertyType.FullName == "System.Int32")
                {
                    isKeyAutoSEQ = true;
                    builderSQL.Append(property.Name + " , ");
                    builderParams.AppendFormat("SEQ_{0}.nextval, ", tableName.ToUpper());
                }
                //NotMapped 是自定义的字段，不属于表，所以要排除
                else if (!AttrAssistant.IsKey(property) && !AttrAssistant.IsNotMapped(property))
                {
                    object obj = property.GetValue(model, null);

                    builderSQL.Append(property.Name + " , ");
                    builderParams.Append(":" + property.Name + " , ");
                    lstParams.Add(MakeParam(":" + property.Name, obj));
                }
            }

            builderSQL.Remove(builderSQL.Length - 2, 2);
            builderParams.Remove(builderParams.Length - 2, 2);

            builderSQL.Append(" ) values ");
            builderSQL.Append(builderParams.ToString() + " ) ");
            dictSql.Add("NOVALUE", (" insert into " + tableName + " ( " + builderSQL.ToString(), lstParams.ToArray()));

            /*
             * 先建立一个序列
             * create sequence SEQ_SINGOO minvalue 1 maxvalue 9999999999 start with 1 increment by 1 nocache
             * insert into cms_user(AutoID,UserName) VALUES(SEQ_SINGOO.nextval,'张三');
             * select SEQ_SINGOO.currval from dual; //取值
             */
            if (isKeyAutoSEQ)
                dictSql.Add("REVALUE", ($"select SEQ_{tableName.ToUpper()}.currval from dual", null));

            var result = ExecOracleTrans(dictSql);
            return (result != null && result.Count() == 2) ? result.ToList()[1] : 0;
        }

        public override async Task<int> InsertModelAsync<T>(T model, string tableName = "")
        {
            var dictSql = new Dictionary<string, (string, DbParameter[])>();

            var arrProperty = typeof(T).GetProperties();
            var builderSQL = new StringBuilder();
            var builderParams = new StringBuilder(" ( ");
            var lstParams = new List<DbParameter>();
            bool isKeyAutoSEQ = false;

            if (tableName.IsNullOrEmpty())
                tableName = AttrAssistant.GetTableName(typeof(T));

            foreach (PropertyInfo property in arrProperty)
            {
                //oracle没有自动增长的列，创建了序列也要主动赋值
                if (AttrAssistant.IsKey(property) && property.PropertyType.FullName == "System.Int32")
                {
                    isKeyAutoSEQ = true;
                    builderSQL.Append(property.Name + " , ");
                    builderParams.Append("SEQ_" + tableName + ".nextval, ");
                }
                //NotMapped 是自定义的字段，不属于表，所以要排除
                else if (!AttrAssistant.IsKey(property) && !AttrAssistant.IsNotMapped(property))
                {
                    object obj = property.GetValue(model, null);

                    builderSQL.Append(property.Name + " , ");
                    builderParams.Append(":" + property.Name + " , ");
                    lstParams.Add(MakeParam(":" + property.Name, obj));
                }
            }

            builderSQL.Remove(builderSQL.Length - 2, 2);
            builderParams.Remove(builderParams.Length - 2, 2);

            builderSQL.Append(" ) values ");
            builderSQL.Append(builderParams.ToString() + " ) ");
            dictSql.Add("NOVALUE", (" insert into " + tableName + " ( " + builderSQL.ToString(), lstParams.ToArray()));

            /*
             * 先建立一个序列
             * create sequence SEQ_SINGOO minvalue 1 maxvalue 9999999999 start with 1 increment by 1 nocache
             * insert into cms_user(AutoID,UserName) VALUES(SEQ_SINGOO.nextval,'张三');
             * select SEQ_SINGOO.currval from dual; //取值
             */
            if (isKeyAutoSEQ)
                dictSql.Add("REVALUE", ($"select SEQ_{tableName.ToUpper()}.currval from dual", null));

            var result = await ExecOracleTransAsync(dictSql);
            return (result != null && result.Count() == 2) ? result.ToList()[1] : 0;
        }

        #endregion

        #region 更新

        /// <summary>
        /// 更新 - oracle参数的形式不同 其它的是@user,oracle的参数是:user
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public override bool UpdateModel<T>(T model)
        {
            var arrProperty = typeof(T).GetProperties();
            var builderSQL = new StringBuilder();
            var lstParams = new List<DbParameter>();

            foreach (PropertyInfo property in arrProperty)
            {
                //NotMapped 是自定义的字段，不属于表，所以要排除
                if (!AttrAssistant.IsKey(property) && !AttrAssistant.IsNotMapped(property))
                {
                    object obj = property.GetValue(model, null);
                    builderSQL.AppendFormat("{0}=:{0} , ", property.Name);
                    lstParams.Add(MakeParam(":" + property.Name, obj));
                }
            }

            builderSQL.Remove(builderSQL.Length - 2, 2);
            string key = AttrAssistant.GetKey(typeof(T));
            if (!key.IsNullOrEmpty())
            {
                //主键值
                object primaryKeyUniqueValue = RefProperty.GetPropertyValue(model, key);
                builderSQL.AppendFormat(" where {0}=:primaryKeyUniqueValue ", key);
                lstParams.Add(MakeParam(":primaryKeyUniqueValue", primaryKeyUniqueValue));
            }

            return ExecSQL(" update " + AttrAssistant.GetTableName(typeof(T)) + " set " + builderSQL.ToString(), lstParams.ToArray());
        }

        /// <summary>
        /// 异步更新 - oracle参数的形式不同 其它的是@user,oracle的参数是:user
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public override async Task<bool> UpdateModelAsync<T>(T model)
        {
            var arrProperty = typeof(T).GetProperties();
            var builderSQL = new StringBuilder();
            var lstParams = new List<DbParameter>();

            foreach (PropertyInfo property in arrProperty)
            {
                //NotMapped 是自定义的字段，不属于表，所以要排除
                if (!AttrAssistant.IsKey(property) && !AttrAssistant.IsNotMapped(property))
                {
                    object obj = property.GetValue(model, null);
                    builderSQL.AppendFormat("{0}=:{0} , ", property.Name);
                    lstParams.Add(MakeParam(":" + property.Name, obj));
                }
            }

            builderSQL.Remove(builderSQL.Length - 2, 2);
            string key = AttrAssistant.GetKey(typeof(T));
            if (!string.IsNullOrEmpty(key))
            {
                //主键值
                object primaryKeyUniqueValue = RefProperty.GetPropertyValue(model, key);
                builderSQL.AppendFormat(" where {0}=:primaryKeyUniqueValue ", key);
                lstParams.Add(MakeParam(":primaryKeyUniqueValue", primaryKeyUniqueValue));
            }

            return await ExecSQLAsync(" update " + AttrAssistant.GetTableName(typeof(T)) + " set " + builderSQL.ToString(), lstParams.ToArray());
        }

        public override bool UpdateColumn<T>(Expression<Func<T, T>> columns, string condition = "", DbParameter[] conditionParameters = null)
        {
            var builder = new StringBuilder();
            var parameters = new List<DbParameter>();
            builder.Append($" update {AttrAssistant.GetTableName(typeof(T))} set ");

            var bindings = (columns.Body as MemberInitExpression).Bindings;
            foreach (var item in bindings)
            {
                builder.AppendFormat("{0}=:{0},", item.Member.Name);
                parameters.Add(MakeParam(":" + item.Member.Name, ((item as MemberAssignment).Expression as ConstantExpression).Value));
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

        public override async Task<bool> UpdateColumnAsync<T>(Expression<Func<T, T>> columns, string condition = "", DbParameter[] conditionParameters = null)
        {
            var builder = new StringBuilder();
            var parameters = new List<DbParameter>();
            builder.Append($" update {AttrAssistant.GetTableName(typeof(T))} set ");

            var bindings = (columns.Body as MemberInitExpression).Bindings;
            foreach (var item in bindings)
            {
                builder.AppendFormat("{0}=:{0},", item.Member.Name);
                parameters.Add(MakeParam(":" + item.Member.Name, ((item as MemberAssignment).Expression as ConstantExpression).Value));
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

        public override bool Delete<T>(object keyValue)
        {
            if (keyValue == null)
                throw new ArgumentException("参数 keyValue 值不能为 null");

            var type = typeof(T);
            string tableName = AttrAssistant.GetTableName(type);
            string key = AttrAssistant.GetKey(type);

            string sql = $" delete from {tableName} where {key}=:{key} ";
            var parameters = new DbParameter[]{
                MakeParam($":{key}", keyValue)
            };

            return ExecSQL(sql, parameters);
        }
        public override async Task<bool> DeleteAsync<T>(object keyValue)
        {
            if (keyValue == null)
                throw new ArgumentException("参数 keyValue 值不能为 null");

            var type = typeof(T);
            string tableName = AttrAssistant.GetTableName(type);
            string key = AttrAssistant.GetKey(type);

            string sql = $" delete from {tableName} where {key}=:{key} ";
            var parameters = new DbParameter[]{
                MakeParam($":{key}", keyValue)
            };

            return await ExecSQLAsync(sql, parameters);
        }

        #endregion

        #endregion        

        #region ------Oracle可选返回值 事务------

        public IEnumerable<int> ExecOracleTrans(Dictionary<string, (string, DbParameter[])> lstSql)
        {
            var lstRets = new List<int>();
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
                                if (item.Key == "REVALUE") //需要返回值
                                {
                                    cmd.CommandText = item.Value.Item1;
                                    if (item.Value.Item2 != null)
                                        cmd.Parameters.AddRange(item.Value.Item2);

                                    var obj = cmd.ExecuteScalar();
                                    if (obj != null && obj != DBNull.Value && int.TryParse(obj.ToString(), out _))
                                        lstRets.Add(int.Parse(obj.ToString()));
                                    else
                                        lstRets.Add(0);
                                }
                                else //不需要返回值
                                {
                                    cmd.CommandText = item.Value.Item1;
                                    if (item.Value.Item2 != null)
                                        cmd.Parameters.AddRange(item.Value.Item2);

                                    lstRets.Add(cmd.ExecuteNonQuery());
                                }
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

            return lstRets;
        }

        public virtual async Task<IEnumerable<int>> ExecOracleTransAsync(Dictionary<string, (string, DbParameter[])> lstSql)
        {
            var lstRets = new List<int>();
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
                                if (item.Key == "REVALUE") //需要返回值
                                {
                                    cmd.CommandText = item.Value.Item1;
                                    if (item.Value.Item2 != null)
                                        cmd.Parameters.AddRange(item.Value.Item2);

                                    var obj = cmd.ExecuteScalar();
                                    if (obj != null && obj != DBNull.Value && int.TryParse(obj.ToString(), out _))
                                        lstRets.Add(int.Parse(obj.ToString()));
                                    else
                                        lstRets.Add(0);
                                }
                                else //不需要返回值
                                {
                                    cmd.CommandText = item.Value.Item1;
                                    if (item.Value.Item2 != null)
                                        cmd.Parameters.AddRange(item.Value.Item2);

                                    lstRets.Add(cmd.ExecuteNonQuery());
                                }
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

            return lstRets;
        }

        #endregion

        #region 其它

        public bool ExisSequence(string sequenceName)
        {
            string sql = $"select count(*) from user_sequences where sequence_name = :seqname";
            var paras = new DbParameter[] { MakeParam(":seqname", sequenceName) };
            return GetValue<int>(sql, paras) > 0;
        }

        public async Task<bool> ExisSequenceAsync(string sequenceName)
        {
            string sql = $"select count(*) from user_sequences where sequence_name = :seqname";
            var paras = new DbParameter[] { MakeParam(":seqname", sequenceName) };
            return await GetValueAsync<int>(sql, paras) > 0;
        }

        public void DelSequence(string sequenceName) =>
            ExecSQL($"drop sequence {sequenceName}");

        public async Task DelSequenceAsync(string sequenceName) =>
            await ExecSQLAsync($"drop sequence {sequenceName}");

        #endregion
    }
}
