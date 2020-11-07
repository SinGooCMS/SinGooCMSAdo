using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using SinGooCMS.Ado.Interface;

namespace SinGooCMS.Ado.DbMaintenance
{
    public class MySqlDbMaintenance : IDbMaintenance
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
            return dbAccess.GetValueList<string>("show tables;");
        }

        public void CreateTable(string tableName, List<DbColumnInfo> columns, bool isCreatePrimaryKey = true)
        {
            string sql = $"create table {tableName} (";
            StringBuilder builder = new StringBuilder();
            foreach (var column in columns)
            {
                builder.Append(string.Format("{0} {1}{2} {3} {4} {5} {6},",
                    column.ColumnName,
                    column.DataType,
                    Utils.IsCharColumn(column.DataType) && column.Length > 0 ? $"({column.Length})" : "",
                    column.IsNullable && !column.IsPrimarykey ? "null" : "not null",
                    column.IsIdentity ? "auto_increment" : "",
                    column.IsPrimarykey ? "primary key" : "",
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
            return dbAccess.ExecSQL($"truncate table {tableName}");
        }
        public bool ExistsTable(string tableName)
        {
            return dbAccess.GetValue<int>($"select count(*) from information_schema.tables where table_name='{tableName}'") > 0;
        }
        #endregion

        #region 列操作

        public IEnumerable<DbColumnInfo> ShowColumns(string tableName)
        {
            var reader = dbAccess.GetDataReader($"describe {tableName};");
            var lst = new List<DbColumnInfo>();
            while (reader.Read())
            {
                //mysql 的类型 显示如 int(11) varchar(50) 类型和长度应该分开
                var dataType = reader.GetString(1);
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
                    ColumnName = reader.GetString(0),
                    DataType = dType,
                    Length = dLength,
                    ColumnDescription = "",
                    DefaultValue = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    IsIdentity = reader.IsDBNull(5) ? false : (reader.GetString(5) == "auto_increment"),
                    IsPrimarykey = reader.IsDBNull(3) ? false : (reader.GetString(3) == "PRI"),
                    IsNullable = reader.IsDBNull(2) ? false : (reader.GetString(2) == "YES")
                });
            }

            return lst;
        }

        public void AddColumn(string tableName, DbColumnInfo column)
        {
            string sql = string.Format("alter table {0} add {1} {2}{3} {4} {5} {6} {7}",
                tableName,
                column.ColumnName,
                column.DataType,
                Utils.IsCharColumn(column.DataType) && column.Length > 0 ? $"({column.Length})" : "",
                column.IsNullable ? "null" : "not null",
                column.IsIdentity ? "auto_increment" : "",
                column.IsPrimarykey ? "primary key" : "",
                !string.IsNullOrEmpty(column.DefaultValue) ? $"default {column.DefaultValue}" : ""
                );

            dbAccess.ExecSQL(sql);
        }
        public void RenameColumn(string tableName, string oldColumnName, string newColumnName)
        {
            var lstColumns = ShowColumns(tableName);
            if (lstColumns != null && lstColumns.Count() > 0)
            {
                var column = lstColumns.Where(p => p.ColumnName == oldColumnName).FirstOrDefault();
                if (column != null)
                {
                    if (Utils.IsCharColumn(column.DataType))
                        dbAccess.ExecSQL($"alter table {tableName} change column {oldColumnName} {newColumnName} {column.DataType}({column.Length})");
                    else
                        dbAccess.ExecSQL($"alter table {tableName} change column {oldColumnName} {newColumnName} {column.DataType}");
                }
            }
        }
        public void UpdateColumn(string tableName, DbColumnInfo column)
        {
            string sql = string.Format("alter table {0} modify column {1} {2}{3} {4} {5} {6} {7}",
                tableName,
                column.ColumnName,
                column.DataType,
                Utils.IsCharColumn(column.DataType) && column.Length > 0 ? $"({column.Length})" : "",
                column.IsNullable ? "null" : "not null",
                column.IsIdentity ? "auto_increment" : "",
                column.IsPrimarykey ? "primary key" : "",
                !string.IsNullOrEmpty(column.DefaultValue) ? $"default {column.DefaultValue}" : ""
                );

            dbAccess.ExecSQL(sql);
        }
        public void DropColumn(string tableName, string columnName)
        {
            dbAccess.ExecSQL($"alter table {tableName} drop column {columnName}");
        }
        public bool ExistsColumn(string tableName, string columnName)
        {
            return dbAccess.GetValue<int>($"select count(*) from information_schema.columns where table_name = '{tableName}' and column_name = '{columnName}'") > 0;
        }

        #endregion

        #region 其它操作

        public void BackupDatabase(string databaseName, string fullFileName)
        {
            dbAccess.ExecSQL($"mysqldump -u root -h (local) -p {databaseName} > {fullFileName}");
        }

        #endregion
    }
}
