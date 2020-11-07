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

        public override IEnumerable<T> GetList<T>(int topNum = 0, string condition = "", string sort = "", string filter = "*")
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

            return GetList<T>(builder.ToString());
        }
        public override async Task<IEnumerable<T>> GetListAsync<T>(int topNum = 0, string condition = "", string sort = "", string filter = "*")
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

            return await GetListAsync<T>(builder.ToString());
        }

        #region 分页

        public override DataTable GetPagerDT(string tableName, string condition, string sort, int pageIndex, int pageSize, string filter = "*")
        {
            if (condition.IsNullOrEmpty())
                condition = "1=1";

            //起始页号
            int startPage = (pageIndex - 1) * pageSize + 1;
            //截止页号
            int endPage = pageIndex * pageSize;

            var builder = new StringBuilder();
            builder.AppendFormat(@"select {0}
                                from(select row_number() over(order by {3}) as rownum,*
                                          from  {1}
                                          where {2}
                                        ) as result
                                where rownum between {4} and {5}
                                order by {3}", filter, tableName, condition, sort, startPage, endPage);

            return GetDataTable(builder.ToString());
        }

        public override IEnumerable<T> GetPagerList<T>(string condition, string sort, int pageIndex, int pageSize, string filter = "*")
        {
            var lstResult = new List<T>();
            T tItem = default(T);            
            string tableName = AttrAssistant.GetTableName(typeof(T)); //表名

            if (condition.IsNullOrEmpty())
                condition = "1=1";

            //起始页号
            int startPage = (pageIndex - 1) * pageSize + 1;
            //截止页号
            int endPage = pageIndex * pageSize;

            var builder = new StringBuilder();
            builder.AppendFormat(@"select {0}
                                from(select row_number() over(order by {3}) as rownum,*
                                          from  {1}
                                          where {2}
                                        ) as result
                                where rownum between {4} and {5}
                                order by {3}", filter, tableName, condition, sort, startPage, endPage);

            var reader = GetDataReader(builder.ToString());
            var refBuilder = ReflectionBuilder<T>.CreateBuilder(reader);
            while (reader.Read())
            {
                tItem = refBuilder.Build(reader, dbProviderType);
                lstResult.Add(tItem);
            }

            reader.Close();
            return lstResult;
        }
        public override async Task<IEnumerable<T>> GetPagerListAsync<T>(string condition, string sort, int pageIndex, int pageSize, string filter = "*")
        {
            var lstResult = new List<T>();
            T tItem = default(T);            
            string tableName = AttrAssistant.GetTableName(typeof(T)); //表名

            if (condition.IsNullOrEmpty())
                condition = "1=1";

            //起始页号
            int startPage = (pageIndex - 1) * pageSize + 1;
            //截止页号
            int endPage = pageIndex * pageSize;

            var builder = new StringBuilder();
            builder.AppendFormat(@"select {0}
                                from(select row_number() over(order by {3}) as rownum,*
                                          from  {1}
                                          where {2}
                                        ) as result
                                where rownum between {4} and {5}
                                order by {3}", filter, tableName, condition, sort, startPage, endPage);

            var reader = await GetDataReaderAsync(builder.ToString());
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

        public override int InsertModel<T>(T model, string tableName)
        {
            var arrProperty = typeof(T).GetProperties();
            var builderSQL = new StringBuilder();
            var builderParams = new StringBuilder(" ( ");
            var lstParams = new List<DbParameter>();

            foreach (PropertyInfo property in arrProperty)
            {
                //NotMapped 是自定义的字段，不属于表，所以要排除 如果是自增Key，也要加上NotMapped，因为有可能key是非自增列
                if (!AttrAssistant.IsNotMapped(property))
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

        public override async Task<int> InsertModelAsync<T>(T model, string tableName)
        {
            var arrProperty = typeof(T).GetProperties();
            var builderSQL = new StringBuilder();
            var builderParams = new StringBuilder(" ( ");
            var lstParams = new List<DbParameter>();

            foreach (PropertyInfo property in arrProperty)
            {
                //NotMapped 是自定义的字段，不属于表，所以要排除 如果是自增Key，也要加上NotMapped，因为有可能key是非自增列
                if (!AttrAssistant.IsNotMapped(property))
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
    }
}
