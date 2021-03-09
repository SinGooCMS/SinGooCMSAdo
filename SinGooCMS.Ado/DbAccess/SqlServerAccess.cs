using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

#if NETSTANDARD2_1
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif

using SinGooCMS.Ado.Interface;

namespace SinGooCMS.Ado.DbAccess
{
    public class SqlServerAccess : DbAccessBase, IDbAccess, ISqlServer
    {
        public SqlServerAccess(string _customConnStr)
            : base(_customConnStr, DbProviderType.SqlServer)
        {
            //
        }

        public SqlServerAccess()
            : this(Utils.DefConnStr)
        {
            //默认连接字符串
        }

        #region CRUD

        #region 查询        

        public override T Find<T>(object keyValue)
        {
            string tableName = AttrAssistant.GetTableName(typeof(T));
            string key = AttrAssistant.GetKey(typeof(T));

            return GetModel<T>(GetDataReader($"select top 1 * from {tableName} where {key} = @KeyValue", new DbParameter[] { MakeParam("@KeyValue", keyValue) }));
        }
        public override async Task<T> FindAsync<T>(object keyValue)
        {
            string tableName = AttrAssistant.GetTableName(typeof(T));
            string key = AttrAssistant.GetKey(typeof(T));

            return GetModel<T>(await GetDataReaderAsync($"select top 1 * from {tableName} where {key} = @KeyValue", new DbParameter[] { MakeParam("@KeyValue", keyValue) }));
        }

        public override IEnumerable<T> GetList<T>(int topNum = 0, string condition = "", string sort = "", string filter = "*", DbParameter[] conditionParameters = null)
        {
            string tableName = AttrAssistant.GetTableName(typeof(T));

            if (filter.IsNullOrEmpty())
                filter = "*";
            if (topNum > 0)
                filter = $" top {topNum} {filter}";

            var builder = new StringBuilder("select ");
            builder.AppendFormat(" {0} from {1} ", filter, tableName);
            if (!condition.IsNullOrEmpty())
                builder.AppendFormat(" where {0} ", condition);
            if (!sort.IsNullOrEmpty())
                builder.AppendFormat(" order by {0} ", sort);

            return GetList<T>(builder.ToString(), conditionParameters);
        }
        public override async Task<IEnumerable<T>> GetListAsync<T>(int topNum = 0, string condition = "", string sort = "", string filter = "*", DbParameter[] conditionParameters = null)
        {
            string tableName = AttrAssistant.GetTableName(typeof(T));

            if (filter.IsNullOrEmpty())
                filter = "*";
            if (topNum > 0)
                filter = $" top {topNum} {filter}";

            var builder = new StringBuilder("select ");
            builder.AppendFormat(" {0} from {1} ", filter, tableName);
            if (!condition.IsNullOrEmpty())
                builder.AppendFormat(" where {0} ", condition);
            if (!sort.IsNullOrEmpty())
                builder.AppendFormat(" order by {0} ", sort);

            return await GetListAsync<T>(builder.ToString(), conditionParameters);
        }

        #region 分页

        public override DataTable GetPagerDT(string tableName, string condition, string sort, int pageIndex, int pageSize, string filter = "*", DbParameter[] conditionParameters = null)
        {
            if (condition.IsNullOrEmpty())
                condition = "1=1";

            //起始记录
            int startPosition = (pageIndex - 1) * pageSize + 1;
            int offsetPosition = (pageIndex - 1) * pageSize;
            //截止记录
            int endPosition = pageIndex * pageSize;

            var builder = new StringBuilder();
            if (GetVerNo().GetAwaiter().GetResult() >= 11)
            {
                builder.AppendFormat(@"select {0}
                                    from {1} as {6}
                                    where {2}
                                    order by {3}
                                    offset {4} rows fetch next {5} rows only",
                                    filter, tableName, condition, sort, offsetPosition, endPosition, Utils.SinGooPagerAlias);
            }
            else
            {
                builder.AppendFormat(@"select {0}
                                from(select row_number() over(order by {3}) as rownum,*
                                    from  {1}
                                    where {2}
                                ) as {6}
                                where rownum between {4} and {5}
                                order by {3}", filter, tableName, condition, sort, startPosition, endPosition, Utils.SinGooPagerAlias);
            }

            return GetDataTable(builder.ToString(), conditionParameters);
        }

        public override IEnumerable<T> GetPagerList<T>(string condition, string sort, int pageIndex, int pageSize, string filter = "*", DbParameter[] conditionParameters = null)
        {
            var lstResult = new List<T>();
            T tItem = default(T);
            string tableName = AttrAssistant.GetTableName(typeof(T)); //表名

            if (condition.IsNullOrEmpty())
                condition = "1=1";

            //起始记录
            int startPosition = (pageIndex - 1) * pageSize + 1;
            int offsetPosition = (pageIndex - 1) * pageSize;
            //截止记录
            int endPosition = pageIndex * pageSize;

            var builder = new StringBuilder();
            if (GetVerNo().GetAwaiter().GetResult() >= 11)
            {
                builder.AppendFormat(@"select {0}
                                    from {1} as {6}
                                    where {2}
                                    order by {3}
                                    offset {4} rows fetch next {5} rows only",
                                    filter, tableName, condition, sort, offsetPosition, endPosition, Utils.SinGooPagerAlias);
            }
            else
            {
                builder.AppendFormat(@"select {0}
                                from(select row_number() over(order by {3}) as rownum,*
                                    from  {1}
                                    where {2}
                                ) as {6}
                                where rownum between {4} and {5}
                                order by {3}", filter, tableName, condition, sort, startPosition, endPosition, Utils.SinGooPagerAlias);
            }

            var reader = GetDataReader(builder.ToString(), conditionParameters);
            var refBuilder = ReflectionBuilder<T>.CreateBuilder(reader);
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

            if (condition.IsNullOrEmpty())
                condition = "1=1";

            //起始记录
            int startPosition = (pageIndex - 1) * pageSize + 1;
            int offsetPosition = (pageIndex - 1) * pageSize;
            //截止记录
            int endPosition = pageIndex * pageSize;

            var builder = new StringBuilder();
            if (await GetVerNo() >= 11)
            {
                builder.AppendFormat(@"select {0}
                                    from {1} as {6}
                                    where {2}
                                    order by {3}
                                    offset {4} rows fetch next {5} rows only",
                                    filter, tableName, condition, sort, offsetPosition, endPosition, Utils.SinGooPagerAlias);
            }
            else
            {
                builder.AppendFormat(@"select {0}
                                from(select row_number() over(order by {3}) as rownum,*
                                    from  {1}
                                    where {2}
                                ) as {6}
                                where rownum between {4} and {5}
                                order by {3}", filter, tableName, condition, sort, startPosition, endPosition, Utils.SinGooPagerAlias);
            }

            var reader = await GetDataReaderAsync(builder.ToString(), conditionParameters);
            var refBuilder = ReflectionBuilder<T>.CreateBuilder(reader);
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
            var arrProperty = typeof(T).GetProperties();
            var builderSQL = new StringBuilder();
            var builderParams = new StringBuilder(" ( ");
            var lstParams = new List<DbParameter>();

            if (tableName.IsNullOrEmpty())
                tableName = AttrAssistant.GetTableName(typeof(T));

            foreach (PropertyInfo property in arrProperty)
            {
                //NotMapped 是自定义的字段，不属于表，所以要排除 如果是自增Key，也要加上NotMapped，因为有可能key是非自增列
                if (!AttrAssistant.IsKey(property) && !AttrAssistant.IsNotMapped(property))
                {
                    object obj = property.GetValue(model, null);

                    builderSQL.Append(property.Name + " , ");
                    builderParams.Append("@" + property.Name + " , ");
                    lstParams.Add(MakeParam("@" + property.Name, obj));
                }
            }

            builderSQL.Remove(builderSQL.Length - 2, 2);
            builderParams.Remove(builderParams.Length - 2, 2);

            builderSQL.Append(" ) values ");
            builderSQL.Append(builderParams.ToString() + " ) ");
            builderSQL.Append(";select @@IDENTITY;"); //不同数据库返回主键值的方式各不相同，所以要重载

            object objTemp = GetObject(" insert into " + tableName + " ( " + builderSQL.ToString(), lstParams.ToArray());
            return objTemp.ToInt();
        }

        public override async Task<int> InsertModelAsync<T>(T model, string tableName = "")
        {
            var arrProperty = typeof(T).GetProperties();
            var builderSQL = new StringBuilder();
            var builderParams = new StringBuilder(" ( ");
            var lstParams = new List<DbParameter>();

            if (tableName.IsNullOrEmpty())
                tableName = AttrAssistant.GetTableName(typeof(T));

            foreach (PropertyInfo property in arrProperty)
            {
                //NotMapped 是自定义的字段，不属于表，所以要排除 如果是自增Key，也要加上NotMapped，因为有可能key是非自增列
                if (!AttrAssistant.IsKey(property) && !AttrAssistant.IsNotMapped(property))
                {
                    object obj = property.GetValue(model, null);

                    builderSQL.Append(property.Name + " , ");
                    builderParams.Append("@" + property.Name + " , ");
                    lstParams.Add(MakeParam("@" + property.Name, obj));
                }
            }

            builderSQL.Remove(builderSQL.Length - 2, 2);
            builderParams.Remove(builderParams.Length - 2, 2);

            builderSQL.Append(" ) values ");
            builderSQL.Append(builderParams.ToString() + " ) ");
            builderSQL.Append(";select @@IDENTITY;"); //不同数据库返回主键值的方式各不相同，所以要重载

            object objTemp = await GetObjectAsync(" insert into " + tableName + " ( " + builderSQL.ToString(), lstParams.ToArray());
            return objTemp.ToInt();
        }

        #region 批量导入        

        public void BulkInsert(DataTable dt, string targetTableName)
        {
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn))
                {
                    bulkCopy.DestinationTableName = targetTableName;
                    bulkCopy.WriteToServer(dt);
                }
            }
        }

        public void BulkInsert<T>(DataTable dt) where T : class
        {
            BulkInsert(dt, AttrAssistant.GetTableName(typeof(T)));
        }
        #endregion

        #endregion

        #region 更新



        #endregion

        #region 删除


        #endregion

        #endregion        

        #region 事务


        #endregion

        #region 执行存储过程


        #endregion

        /// <summary>
        /// 读取版本号 sql2008=10,sql2012=11,sql2019=15;sql2012以上支持offset fatch next分页方式
        /// </summary>
        /// <returns></returns>
        private async Task<int> GetVerNo()
        {
            return await GetValueAsync<int>("select left(cast(SERVERPROPERTY('ProductVersion') as varchar),2)");
        }
    }
}
