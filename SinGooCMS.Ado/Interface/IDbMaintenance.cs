using System;
using System.Collections.Generic;
using System.Data.Common;

namespace SinGooCMS.Ado.Interface
{
    /// <summary>
    /// 数据维护
    /// </summary>
    public interface IDbMaintenance
    {
        /// <summary>
        /// 设置操作源
        /// </summary>
        /// <param name="_dbAccess"></param>
        /// <returns></returns>
        IDbMaintenance Set(IDbAccess _dbAccess);

        #region 表操作

        /// <summary>
        /// 显示所有表名
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> ShowTables();

        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <param name="isCreatePrimaryKey"></param>
        /// <returns></returns>
        void CreateTable(string tableName, List<DbColumnInfo> columns, bool isCreatePrimaryKey = true);
        /// <summary>
        /// 删除表
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        void DropTable(string tableName);
        /// <summary>
        /// 清空表
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        bool TruncateTable(string tableName);
        /// <summary>
        /// 表是否存在
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        bool ExistsTable(string tableName);

        #endregion

        #region 列操作

        /// <summary>
        /// 显示所有列
        /// </summary>
        /// <returns></returns>
        IEnumerable<DbColumnInfo> ShowColumns(string tableName);

        /// <summary>
        /// 增加列
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        void AddColumn(string tableName, DbColumnInfo column);
        /// <summary>
        /// 更新列
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        void UpdateColumn(string tableName, DbColumnInfo column);
        /// <summary>
        /// 修改列名
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="oldColumnName"></param>
        /// <param name="newColumnName"></param>
        /// <returns></returns>
        void RenameColumn(string tableName, string oldColumnName, string newColumnName);
        /// <summary>
        /// 删除列
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        void DropColumn(string tableName, string columnName);
        /// <summary>
        /// 是否存在列
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        bool ExistsColumn(string tableName, string columnName);

        #endregion

        #region 其它操作

        /// <summary>
        /// 备份数据库（注意权限问题，一般的空间商都没有提供这个权限）
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="fullFileName"></param>
        /// <returns></returns>
        void BackupDatabase(string databaseName, string fullFileName);
        #endregion
    }
}
