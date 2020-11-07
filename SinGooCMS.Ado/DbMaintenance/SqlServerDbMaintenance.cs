using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

#if NETSTANDARD2_1
using Microsoft.Data.SqlClient;
#endif

using SinGooCMS.Ado.Interface;

namespace SinGooCMS.Ado.DbMaintenance
{
    public class SqlServerDbMaintenance : IDbMaintenance
    {
        private IDbAccess dbAccess = DbProvider.DbAccess;
        public IDbMaintenance Set(IDbAccess _dbAccess)
        {
            this.dbAccess = _dbAccess;
            return this;
        }

        #region 表操作

        public IEnumerable<string> ShowTables()
            => dbAccess.GetValueList<string>("SELECT Name FROM SysObjects Where XType='U' ORDER BY Name");

        public void CreateTable(string tableName, List<DbColumnInfo> columns, bool isCreatePrimaryKey = true)
        {
            string sql = $"create table {tableName} (";
            var builder = new StringBuilder();
            var descBuilder = new StringBuilder();

            foreach (var column in columns)
            {
                builder.Append(string.Format("{0} {1}{2} {3} {4} {5} {6},",
                    column.ColumnName,
                    column.DataType,
                    Utils.IsCharColumn(column.DataType) && column.Length > 0 ? $"({column.Length})" : "",
                    column.IsNullable && !column.IsPrimarykey ? "null" : "not null",
                    column.IsIdentity ? "identity" : "",
                    column.IsPrimarykey ? "primary key" : "",
                    !string.IsNullOrEmpty(column.DefaultValue) ? $"default {column.DefaultValue}" : ""
                    ));

                if (!column.ColumnDescription.IsNullOrEmpty())
                {
                    //添加数据库 注释说明
                    descBuilder.AppendFormat("EXECUTE sp_addextendedproperty N'MS_Description', N'{0}', N'user', N'dbo', N'table', N'{1}', N'column', N'{2}';", column.ColumnDescription, tableName, column.ColumnName);
                }
            }

            sql = sql + builder.ToString().TrimEnd(',') + ")";
            sql = sql + ";" + descBuilder.ToString();
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
            return dbAccess.GetValue<int>($"select object_id('{tableName}')") > 0;
        }

        #endregion

        #region 列操作

        public IEnumerable<DbColumnInfo> ShowColumns(string tableName)
        {
            string sql = string.Format(@"SELECT a.name as ColumnName,b.name as DataType,
                                        COLUMNPROPERTY(a.id,a.name,'PRECISION') as [Length],
                                        isnull(g.[value], ' ') AS ColumnDescription,
                                        isnull(e.text,'') as DefaultValue,
                                        (case when COLUMNPROPERTY( a.id,a.name,'IsIdentity')=1 then cast(1 as bit) else cast(0 as bit) end) as IsIdentity,
                                        (case when (SELECT count(*) FROM sysobjects
                                        WHERE (name in (SELECT name FROM sysindexes
                                        WHERE (id = a.id) AND (indid in
                                        (SELECT indid FROM sysindexkeys
                                        WHERE (id = a.id) AND (colid in
                                        (SELECT colid FROM syscolumns WHERE (id = a.id) AND (name = a.name)))))))
                                        AND (xtype = 'PK'))>0 then cast(1 as bit) else cast(0 as bit) end) as IsPrimarykey,
                                        (case when a.isnullable=1 then cast(1 as bit) else cast(0 as bit) end) as IsNullable
                                        FROM syscolumns a
                                        left join systypes b on a.xtype=b.xusertype
                                        inner join sysobjects d on a.id=d.id and d.xtype='U' and d.name<>'dtproperties'
                                        left join syscomments e on a.cdefault=e.id
                                        left join sys.extended_properties g on a.id=g.major_id AND a.colid=g.minor_id
                                        left join sys.extended_properties f on d.id=f.class and f.minor_id=0
                                        where b.name is not null
                                        and d.name='{0}'
                                        order by a.id,a.colorder", tableName);

            return dbAccess.GetList<DbColumnInfo>(sql);
        }

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
            dbAccess.ExecSQL($"exec sp_rename '{tableName}.{oldColumnName}','{newColumnName}','column';");
        }
        public void UpdateColumn(string tableName, DbColumnInfo column)
        {
            string sql = string.Format("alter table {0} alter column {1} {2}{3} {4} {5} {6} {7}",
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
        public void DropColumn(string tableName, string columnName)
        {
            dbAccess.ExecSQL($"alter table {tableName} drop column {columnName}");
        }
        public bool ExistsColumn(string tableName, string columnName)
        {
            return dbAccess.GetValue<int>($"select COL_LENGTH('{tableName}', '{columnName}')") > 0;
        }

        #endregion

        #region 其它操作

        public void BackupDatabase(string databaseName, string fullFileName)
        {
            dbAccess.ExecSQL($"USE {databaseName};BACKUP DATABASE {databaseName} TO disk = '{fullFileName}'");
        }

        #endregion
    }
}
