using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using SinGooCMS.Ado.Interface;

namespace SinGooCMS.Ado.DbMaintenance
{
    public class SqliteDbMaintenance : IDbMaintenance
    {
        private IDbAccess dbAccess = DbProvider.DbAccess;
        public IDbMaintenance Set(IDbAccess _dbAccess)
        {
            this.dbAccess = _dbAccess;
            return this;
        }

        #region 表操作

        public IEnumerable<string> ShowTables()
        {
            //sqlite常常会有自动创建一些辅助的表，实际应用中要排除掉
            return dbAccess.GetValueList<string>("select name from sqlite_master where type='table' order by name;");
        }

        public void CreateTable(string tableName, List<DbColumnInfo> columns, bool isCreatePrimaryKey = true)
        {
            string sql = $"create table {tableName} (";
            StringBuilder builder = new StringBuilder();
            foreach (var column in columns)
            {
                string dataType = column.DataType.ToLower();
                switch(column.DataType.ToLower())
                {
                    case "int":
                        dataType = "INTEGER";
                        break;
                }

                builder.Append(string.Format("{0} {1}{2} {3} {4} {5} {6},",
                    column.ColumnName,
                    dataType,
                    Utils.IsCharColumn(column.DataType) && column.Length > 0 ? $"({column.Length})" : "",
                    column.IsNullable && !column.IsPrimarykey ? "null" : "not null",
                    column.IsPrimarykey ? "primary key" : "",
                    column.IsIdentity ? "autoincrement" : "",                    
                    !string.IsNullOrEmpty(column.DefaultValue) ? $"default {column.DefaultValue}" : ""
                    ));
            }
            sql = sql + builder.ToString().TrimEnd(',') + ")";
            dbAccess.ExecSQL(sql);
        }
        public void DropTable(string tableName)
        {
            dbAccess.ExecSQL($"drop table {tableName}");
        }
        public bool TruncateTable(string tableName)
        {
            return dbAccess.ExecSQL($"delete from {tableName};update sqlite_sequence set seq = 0 where name = '{tableName}';");
        }
        public bool ExistsTable(string tableName)
        {
            //注意表名区分大小写，一定要按数据库里实际的表名
            return dbAccess.GetValue<int>($"select count(*) from sqlite_master where type='table' AND name = '{tableName}'") > 0;
        }

        #endregion

        #region 列操作

        public IEnumerable<DbColumnInfo> ShowColumns(string tableName)
        {
            var reader = dbAccess.GetDataReader($"PRAGMA table_info('{tableName}')");
            var lst = new List<DbColumnInfo>();
            while (reader.Read())
            {
                //sqlite 的类型 显示如 INTEGER VARCHAR(50) 类型和长度应该分开
                var dataType = reader.GetString(2);
                string dType = dataType;
                int dLength = 0;

                if (dType.IndexOf("(") > 0)
                {
                    dType = dataType.Substring(0, dataType.IndexOf("("));
                    if (dataType.IndexOf(",") != -1)
                        dLength = dataType.Substring(dataType.IndexOf("(") + 1, dataType.IndexOf(",") - dataType.IndexOf("(") - 1).ToInt();
                    else
                        dLength = dataType.Substring(dataType.IndexOf("(") + 1, dataType.IndexOf(")") - dataType.IndexOf("(") - 1).ToInt();
                }

                lst.Add(new DbColumnInfo()
                {
                    ColumnName = reader.GetString(1),
                    DataType = dType,
                    Length = dLength,
                    ColumnDescription = "",
                    DefaultValue = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    IsIdentity = reader.IsDBNull(5) ? false : reader.GetBoolean(5),
                    IsPrimarykey = reader.IsDBNull(5) ? false : reader.GetBoolean(5),
                    IsNullable = reader.IsDBNull(3) ? false : reader.GetBoolean(3)
                });
            }

            return lst;
        }

        /* 注意 sqlite 只能添加列，修改和删除都特别麻烦
         * 只能重新建立一个表，把原表数据导入，并不能直接修改删除列
         */
        public void AddColumn(string tableName, DbColumnInfo column)
        {
            string sql = string.Format("alter table {0} add {1} {2}{3} {4} {5} {6} {7}",
                tableName,
                column.ColumnName,
                column.DataType,
                Utils.IsCharColumn(column.DataType) && column.Length > 0 ? $"({column.Length})" : "",
                column.IsNullable ? "null" : "not null",
                column.IsIdentity ? "identity" : "",
                column.IsPrimarykey ? "primary key" : "",
                !string.IsNullOrEmpty(column.DefaultValue) ? $"default {column.DefaultValue}" : ""
                );

            dbAccess.ExecSQL(sql);
        }
        public void RenameColumn(string tableName, string oldColumnName, string newColumnName)
        {
            throw new Exception("sqlite 不支持更改列名！");
        }
        public void UpdateColumn(string tableName, DbColumnInfo column)
        {
            throw new Exception("sqlite 不支持更新列！");
        }
        public void DropColumn(string tableName, string columnName)
        {
            throw new Exception("sqlite 不支持删除列！");
        }
        public bool ExistsColumn(string tableName, string columnName)
        {
            return dbAccess.GetValue<int>($"select count(*) from sqlite_master where name='{tableName}' and sql like '%{columnName}%'')") > 0;
        }

        #endregion

        #region 其它操作

        public void BackupDatabase(string databaseName, string fullFileName)
        {
            throw new Exception("sqlite 不需要备份！");
        }

        #endregion
    }
}
