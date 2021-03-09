using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using SinGooCMS.Ado.Interface;

namespace SinGooCMS.Ado.DbAccess
{
    public class SqliteAccess : DbAccessBase, IDbAccess
    {
        public SqliteAccess(string _customConnStr)
            : base(_customConnStr, DbProviderType.Sqlite)
        {
            //
        }

        public SqliteAccess() :
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

            return GetModel<T>(GetDataReader($"select * from {tableName} where {key} = @KeyValue limit 1", new DbParameter[] { MakeParam("@KeyValue", keyValue) }));
        }
        public override async Task<T> FindAsync<T>(object keyValue)
        {
            string tableName = AttrAssistant.GetTableName(typeof(T));
            string key = AttrAssistant.GetKey(typeof(T));

            return GetModel<T>(await GetDataReaderAsync($"select * from {tableName} where {key} = @KeyValue limit 1", new DbParameter[] { MakeParam("@KeyValue", keyValue) }));
        }

        public override IEnumerable<T> GetList<T>(int topNum = 0, string condition = "", string sort = "", string filter = "*", DbParameter[] conditionParameters = null)
        {
            string tableName = AttrAssistant.GetTableName(typeof(T));
            var builder = new StringBuilder("select ");

            if (string.IsNullOrEmpty(filter))
                filter = "*";

            builder.AppendFormat(" {0} from {1} ", filter, tableName);
            if (!condition.IsNullOrEmpty())
                builder.AppendFormat(" where {0} ", condition);
            if (!sort.IsNullOrEmpty())
                builder.AppendFormat(" order by {0} ", sort);
            if (topNum > 0)
                builder.AppendFormat(" limit {0} ", topNum);

            return GetList<T>(builder.ToString(), conditionParameters);
        }
        public override async Task<IEnumerable<T>> GetListAsync<T>(int topNum = 0, string condition = "", string sort = "", string filter = "*", DbParameter[] conditionParameters = null)
        {
            string tableName = AttrAssistant.GetTableName(typeof(T));
            var builder = new StringBuilder("select ");

            if (string.IsNullOrEmpty(filter))
                filter = "*";

            builder.AppendFormat(" {0} from {1} ", filter, tableName);
            if (!condition.IsNullOrEmpty())
                builder.AppendFormat(" where {0} ", condition);
            if (!sort.IsNullOrEmpty())
                builder.AppendFormat(" order by {0} ", sort);
            if (topNum > 0)
                builder.AppendFormat(" limit {0} ", topNum);

            return await GetListAsync<T>(builder.ToString(), conditionParameters);
        }

        #region 分页

        public override DataTable GetPagerDT(string tableName, string condition, string sort, int pageIndex, int pageSize, string filter = "*", DbParameter[] conditionParameters = null)
        {
            if (string.IsNullOrEmpty(condition))
                condition = "1=1";

            //起始记录（不包括） 比如 limit 10 offset 10 表示从第11行开始取10条记录
            int startPosition = (pageIndex - 1) * pageSize;

            StringBuilder builder = new StringBuilder();
            //offset代表从第几条记录“之后“开始查询，limit表明查询多少条结果
            builder.AppendFormat(@"select {0} from {1} as {6} {2} {3} limit {4} offset {5}",
                filter, tableName,
                condition.IsNullOrEmpty() ? "" : " where " + condition,
                sort.IsNullOrEmpty() ? "" : " order by " + sort, pageSize, startPosition, Utils.SinGooPagerAlias);

            return GetDataTable(builder.ToString(), conditionParameters);
        }

        public override IEnumerable<T> GetPagerList<T>(string condition, string sort, int pageIndex, int pageSize, string filter = "*", DbParameter[] conditionParameters = null)
        {
            var lstResult = new List<T>();
            T tItem = default(T);
            string tableName = AttrAssistant.GetTableName(typeof(T)); //表名

            if (string.IsNullOrEmpty(condition))
                condition = "1=1";

            //起始记录(不包括)
            int startPosition = (pageIndex - 1) * pageSize;

            StringBuilder builder = new StringBuilder();
            //offset代表从第几条记录“之后“开始查询，limit表明查询多少条结果
            builder.AppendFormat(@"select {0} from {1} as {6} {2} {3} limit {4} offset {5}",
                filter, tableName,
                condition.IsNullOrEmpty() ? "" : " where " + condition,
                sort.IsNullOrEmpty() ? "" : " order by " + sort, pageSize, startPosition, Utils.SinGooPagerAlias);

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

            if (string.IsNullOrEmpty(condition))
                condition = "1=1";

            //起始记录(不包括)
            int startPosition = (pageIndex - 1) * pageSize;

            StringBuilder builder = new StringBuilder();
            //offset代表从第几条记录“之后“开始查询，limit表明查询多少条结果
            builder.AppendFormat(@"select {0} from {1} as {6} {2} {3} limit {4} offset {5}",
                filter, tableName,
                condition.IsNullOrEmpty() ? "" : " where " + condition,
                sort.IsNullOrEmpty() ? "" : " order by " + sort, pageSize, startPosition, Utils.SinGooPagerAlias);

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
            builderSQL.Append(";select last_insert_rowid();"); //返回最新的ID

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
            builderSQL.Append(";select last_insert_rowid();"); //返回最新的ID

            object objTemp = await GetObjectAsync(" insert into " + tableName + " ( " + builderSQL.ToString(), lstParams.ToArray());
            return objTemp.ToInt();
        }

        #endregion

        #region 更新



        #endregion

        #region 删除



        #endregion

        #endregion

        #region 事务


        #endregion

        #region 执行存储过程

        //sqlite没有存储过程

        #endregion
    }
}
