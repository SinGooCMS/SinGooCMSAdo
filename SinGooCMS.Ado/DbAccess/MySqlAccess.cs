﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using SinGooCMS.Ado.Interface;

namespace SinGooCMS.Ado.DbAccess
{
    public class MySqlAccess : DbAccessBase, IDbAccess, IMySql
    {
        public MySqlAccess(string _customConnStr, int _dbVersionNo = 0)
            : base(_customConnStr, DbProviderType.MySql, _dbVersionNo)
        {
            //
        }

        public MySqlAccess() :
            this(Utils.DefConnStr, Utils.DbVersionNo)
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

            if (filter.IsNullOrEmpty())
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

            if (filter.IsNullOrEmpty())
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
            if (condition.IsNullOrEmpty())
                condition = "1=1";

            //起始记录
            //int startPosition = (pageIndex - 1) * pageSize + 1;
            int startPosition = (pageIndex - 1) * pageSize; //比如第一页是从0开始，而不是1，和sqlserver不同

            var builder = new StringBuilder();
            //limit 起始行（包括）,记录数
            builder.AppendFormat(@"select {0} from {1} as {6} {2} {3} limit {4},{5}",
                filter, tableName,
                condition.IsNullOrEmpty() ? "" : " where " + condition,
                sort.IsNullOrEmpty() ? "" : " order by " + sort, startPosition, pageSize, Utils.SinGooPagerAlias);

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
            int startPosition = (pageIndex - 1) * pageSize; //比如第一页是从0开始，而不是1，和sqlserver不同

            var builder = new StringBuilder();
            //limit 起始行（包括）,记录数
            builder.AppendFormat(@"select {0} from {1} as {6} {2} {3} limit {4},{5}",
                filter, tableName,
                condition.IsNullOrEmpty() ? "" : " where " + condition,
                sort.IsNullOrEmpty() ? "" : " order by " + sort, startPosition, pageSize, Utils.SinGooPagerAlias);

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
            int startPosition = (pageIndex - 1) * pageSize; //比如第一页是从0开始，而不是1，和sqlserver不同

            var builder = new StringBuilder();
            //limit 起始行（包括）,记录数
            builder.AppendFormat(@"select {0} from {1} as {6} {2} {3} limit {4},{5}",
                filter, tableName,
                condition.IsNullOrEmpty() ? "" : " where " + condition,
                sort.IsNullOrEmpty() ? "" : " order by " + sort, startPosition, pageSize, Utils.SinGooPagerAlias);

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
                //NotMapped 是自定义的字段，不属于表，所以要排除，Key默认是有值的，自增int或者GUID
                if (AttrAssistant.IsKey(property) || AttrAssistant.IsNotMapped(property))
                    continue;

                object obj = property.GetValue(model, null);
                if (obj == null)
                    continue; //null无法加入到参数，跳过

                //日期类型没有值时，默认0001-1-1报错：SqlDateTime overflow. Must be between 1/1/1753 12:00:00 AM and 12/31/9999 11:59:59 PM
                //因此设置新的默认值是 1900-1-1
                if (property.PropertyType.Name.Equals("DateTime") && ((DateTime)obj).Equals(new DateTime(0001, 1, 1)))
                    obj = new DateTime(1900, 1, 1);

                builderSQL.Append(property.Name + " , ");
                builderParams.Append("@" + property.Name + " , ");
                lstParams.Add(MakeParam("@" + property.Name, obj));
            }

            builderSQL.Remove(builderSQL.Length - 2, 2);
            builderParams.Remove(builderParams.Length - 2, 2);

            builderSQL.Append(" ) values ");
            builderSQL.Append(builderParams.ToString() + " ) ");
            builderSQL.Append(";select @@IDENTITY;"); //返回最新的ID

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
                //NotMapped 是自定义的字段，不属于表，所以要排除，Key默认是有值的，自增int或者GUID
                if (AttrAssistant.IsKey(property) || AttrAssistant.IsNotMapped(property))
                    continue;

                object obj = property.GetValue(model, null);
                if (obj == null)
                    continue; //null无法加入到参数，跳过

                //日期类型没有值时，默认0001-1-1报错：SqlDateTime overflow. Must be between 1/1/1753 12:00:00 AM and 12/31/9999 11:59:59 PM
                //因此设置新的默认值是 1900-1-1
                if (property.PropertyType.Name.Equals("DateTime") && ((DateTime)obj).Equals(new DateTime(0001, 1, 1)))
                    obj = new DateTime(1900, 1, 1);

                builderSQL.Append(property.Name + " , ");
                builderParams.Append("@" + property.Name + " , ");
                lstParams.Add(MakeParam("@" + property.Name, obj));
            }

            builderSQL.Remove(builderSQL.Length - 2, 2);
            builderParams.Remove(builderParams.Length - 2, 2);

            builderSQL.Append(" ) values ");
            builderSQL.Append(builderParams.ToString() + " ) ");
            builderSQL.Append(";select @@IDENTITY;"); //返回最新的ID

            object objTemp = await GetObjectAsync(" insert into " + tableName + " ( " + builderSQL.ToString(), lstParams.ToArray());
            return objTemp.ToInt();
        }

        #region 批量导入        

        public void BulkInsert(DataTable dt, string targetTableName)
        {
            dt.TableName = targetTableName;//数据库中的表名
            if (dt.Rows.Count > 0)
            {
                string tmpPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "upload\\temp\\");
                if (!Directory.Exists(tmpPath))
                    Directory.CreateDirectory(tmpPath);

                tmpPath = Path.Combine(tmpPath, "BulkInsertTemp.csv");//csv文件临时目录

                string csv = DataTableToCsv(dt);
                File.WriteAllText(tmpPath, csv);

                var columns = dt.Columns.Cast<DataColumn>().Select(_columns => _columns.ColumnName).ToList();
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();

                    MySqlBulkLoader bulk = new MySqlBulkLoader(conn)
                    {
                        FieldTerminator = ",",
                        FieldQuotationCharacter = '"',
                        EscapeCharacter = '"',
                        LineTerminator = "\r\n",
                        FileName = tmpPath,
                        NumberOfLinesToSkip = 0,
                        TableName = dt.TableName,

                    };
                    bulk.Columns.AddRange(columns);//根据标题列对应插入
                    bulk.Load();
                }
                File.Delete(tmpPath);
            }
        }

        public void BulkInsert<T>(DataTable dt) where T : class
        {
            BulkInsert(dt, AttrAssistant.GetTableName(typeof(T)));
        }

        private string DataTableToCsv(DataTable table)
        {
            //以半角逗号（即,）作分隔符，列为空也要表达其存在。  
            //列内容如存在半角逗号（即,）则用半角引号（即""）将该字段值包含起来。  
            //列内容如存在半角引号（即"）则应替换成半角双引号（""）转义，并用半角引号（即""）将该字段值包含起来。  
            StringBuilder sb = new StringBuilder();
            DataColumn colum;
            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    colum = table.Columns[i];
                    if (i != 0) sb.Append(",");
                    if (colum.DataType == typeof(string) && row[colum].ToString().Contains(","))
                    {
                        sb.Append("\"" + row[colum].ToString().Replace("\"", "\"\"") + "\"");
                    }
                    else sb.Append(row[colum].ToString());
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        #endregion

        #endregion

        #region 更新

        //

        #endregion

        #region 删除

        //

        #endregion

        #endregion

        #region 事务


        #endregion

        #region 存储过程



        #endregion
    }
}
