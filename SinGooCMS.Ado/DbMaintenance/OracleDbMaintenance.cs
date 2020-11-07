using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using SinGooCMS.Ado.Interface;

namespace SinGooCMS.Ado.DbMaintenance
{
    public class OracleDbMaintenance : IDbMaintenance
    {
        private IDbAccess dbAccess = DbProvider.DbAccess;
        public IDbMaintenance Set(IDbAccess _dbAccess)
        {
            this.dbAccess = _dbAccess;
            return this;
        }

        #region 表操作

        public IEnumerable<string> ShowTables()
            => this.dbAccess.GetValueList<string>("select table_name from user_tables");

        public void CreateTable(string tableName, List<DbColumnInfo> columns, bool isCreatePrimaryKey = true)
        {
            var lstSql = new List<string>();

            //注意，ORACLE创建自增加主键，要先创建一个序列，为每个表都单独创建一个序列 序列名称为 SET_大写表名
            StringBuilder builder = new StringBuilder($" create table {tableName} (");
            foreach (var column in columns)
            {
                builder.Append(string.Format("{0} {1}{2} {3} {4} {5},",
                    column.ColumnName,
                    column.DataType,
                    Utils.IsCharColumn(column.DataType) && column.Length > 0 ? $"({column.Length})" : "",
                    column.IsNullable && !column.IsPrimarykey ? "null" : "not null",
                    //column.IsIdentity ? "identity" : "",
                    column.IsPrimarykey ? "primary key" : "",
                    !string.IsNullOrEmpty(column.DefaultValue) ? $"default {column.DefaultValue}" : ""
                    ));
            }
            lstSql.Add(builder.ToString().TrimEnd(',') + ") ");

            //是否有自增加列
            var identityColumn = columns.Where(p => p.IsIdentity).FirstOrDefault();
            if (identityColumn != null)
            {
                //生成一个SEQ_表名大写 的序列 如 SEQ_CMS_USER，在insert数据时用到
                lstSql.Add($" create sequence SEQ_{tableName.ToUpper()} minvalue 1 maxvalue 99999999999 start with 1 increment by 1 nocache ");
            }

            dbAccess.ExecTrans(lstSql);
        }
        public void DropTable(string tableName)
        {
            //删除表 并且删除序列
            dbAccess.ExecSQL($"drop table {tableName};drop sequence SEQ_{tableName.ToUpper()};");
        }
        public bool TruncateTable(string tableName)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("truncate table {0};", tableName);
            builder.AppendFormat("drop sequence {0};", "SEQ_" + tableName.ToUpper()); //删除序列
            builder.AppendFormat("create sequence SEQ_{0} minvalue 1 maxvalue 99999999999 start with 1 increment by 1 nocache;", tableName.ToUpper()); //重置了表，还需要把序列重置

            return dbAccess.ExecSQL(builder.ToString());
        }
        public bool ExistsTable(string tableName)
        {
            return dbAccess.GetValue<int>($"select count(*) from user_tables where table_name =upper('{tableName}')") > 0;
        }

        #endregion

        #region 列操作

        public IEnumerable<DbColumnInfo> ShowColumns(string tableName)
        {
            string sql = string.Format(@"select COLUMN_NAME as ColumnName,DATA_TYPE as DataType,DATA_LENGTH as Length,'' as ColumnDescription,'' as DefaultValue,
                                        0,
                                        (
	                                        case when t.COLUMN_NAME=(select col.column_name from user_constraints con, user_cons_columns col
	                                        where con.constraint_name = col.constraint_name and con.constraint_type = 'P' and col.table_name = '{0}')
	                                        then 1 else 0 end
                                        ) as IsPrimarykey,(case when NULLABLE='Y' then 1 else 0 end) as IsNullable
                                        from user_tab_columns t where Table_Name='{0}';", tableName.ToUpper());

            return dbAccess.GetList<DbColumnInfo>(sql);
        }

        public void AddColumn(string tableName, DbColumnInfo column)
        {
            string sql = string.Format("alter table {0} add {1} {2}{3} {4} {5} {6}",
                tableName,
                column.ColumnName,
                column.DataType,
                Utils.IsCharColumn(column.DataType) && column.Length > 0 ? $"({column.Length})" : "",
                column.IsNullable ? "null" : "not null",
                //column.IsIdentity ? "identity" : "",
                column.IsPrimarykey ? "primary key" : "",
                !string.IsNullOrEmpty(column.DefaultValue) ? $"default {column.DefaultValue}" : ""
                );

            dbAccess.ExecSQL(sql);
        }
        public void RenameColumn(string tableName, string oldColumnName, string newColumnName)
        {
            dbAccess.ExecSQL($"alter {tableName} rename column {oldColumnName} to {newColumnName};");
        }
        public void UpdateColumn(string tableName, DbColumnInfo column)
        {
            string sql = string.Format("alter table {0} modify column {1} {2}{3} {4} {5} {6}",
                tableName,
                column.ColumnName,
                column.DataType,
                Utils.IsCharColumn(column.DataType) && column.Length > 0 ? $"({column.Length})" : "",
                column.IsNullable ? "null" : "not null",
                //column.IsIdentity ? "identity" : "",
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
            return dbAccess.GetValue<int>($"select count(1) from all_tab_columns where table_name='{tableName.ToUpper()}' and column_name='{columnName.ToUpper()}'") > 0;
        }

        #endregion

        #region 其它操作

        public void BackupDatabase(string databaseName, string fullFileName)
        {
            //待完善
        }

        #endregion
    }
}