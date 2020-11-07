using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SinGooCMS.Ado.Interface
{
    /// <summary>
    /// 数据操作
    /// </summary>
    public interface IDbAccess
    {
        #region --------参数----------

        /// <summary>
        /// 创建参数
        /// </summary>
        /// <param name="parameterName">参数名称</param>
        /// <param name="value">参数值</param>
        /// <param name="parameterDirection">参数方向 Input/Output</param>
        /// <returns></returns>
        DbParameter MakeParam(string parameterName, object value, ParameterDirection parameterDirection = ParameterDirection.Input);

        #endregion

        #region ------执行SQL语句------

        /// <summary>
        /// 执行一条SQL语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        bool ExecSQL(string sql, DbParameter[] parameters = null);
        /// <summary>
        /// 异步执行一条SQL语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        Task<bool> ExecSQLAsync(string sql, DbParameter[] parameters = null);

        #endregion        

        #region ------查询操作------

        /// <summary>
        /// 返回一个datareader
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数集合</param>
        /// <returns></returns>
        IDataReader GetDataReader(string sql, DbParameter[] parameters = null);
        /// <summary>
        /// 异步返回一个datareader
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数集合</param>
        /// <returns></returns>
        Task<IDataReader> GetDataReaderAsync(string sql, DbParameter[] parameters = null);

        /// <summary>
        /// 返回一个dataset
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数集合</param>
        /// <returns></returns>
        DataSet GetDataSet(string sql, DbParameter[] parameters = null);

        /// <summary>
        /// 返回一个datatable
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数集合</param>
        /// <returns></returns>
        DataTable GetDataTable(string sql, DbParameter[] parameters = null);

        /// <summary>
        /// 返回一个值
        /// </summary>
        /// <typeparam name="T">泛型：基础类型 如 string int</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数集合</param>
        /// <returns></returns>
        T GetValue<T>(string sql, DbParameter[] parameters = null);
        /// <summary>
        /// 异步返回一个值
        /// </summary>
        /// <typeparam name="T">泛型：基础类型 如 string int</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数集合</param>
        /// <returns></returns>
        Task<T> GetValueAsync<T>(string sql, DbParameter[] parameters = null);

        /// <summary>
        /// 返回值列表
        /// </summary>
        /// <typeparam name="T">泛型：基础类型 如 string int</typeparam>
        /// <param name="sql">查询单个字段</param>
        /// <param name="parameters">参数集合</param>
        /// <returns></returns>
        IEnumerable<T> GetValueList<T>(string sql, DbParameter[] parameters = null);
        /// <summary>
        /// 异步返回值列表
        /// </summary>
        /// <typeparam name="T">泛型：基础类型 如 string int</typeparam>
        /// <param name="sql">查询单个字段</param>
        /// <param name="parameters">参数集合</param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetValueListAsync<T>(string sql, DbParameter[] parameters = null);


        #region 返回一个对象实例

        /// <summary>
        /// 返回一个对象实例
        /// </summary>
        /// <typeparam name="T">实体</typeparam>
        /// <param name="reader">数据流</param>
        /// <returns></returns>
        T GetModel<T>(IDataReader reader) where T : class;

        /// <summary>
        /// 返回一个对象实例
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数集合</param>
        /// <returns></returns>
        T GetModel<T>(string sql, DbParameter[] parameters = null) where T : class;
        /// <summary>
        /// 异步返回一个对象实例
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数集合</param>
        /// <returns></returns>
        Task<T> GetModelAsync<T>(string sql, DbParameter[] parameters = null) where T : class;

        /// <summary>
        /// 返回一个对象实例
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="keyValue">主键值（一般是自增主键）</param>
        /// <returns></returns>
        T Find<T>(object keyValue) where T : class;
        /// <summary>
        /// 异步返回一个对象实例
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="keyValue">主键值（一般是自增主键）</param>
        /// <returns></returns>
        Task<T> FindAsync<T>(object keyValue) where T : class;

        #endregion

        #region 返回一个IList

        /// <summary>
        /// 返回一个泛型集合
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数集合</param>
        /// <returns></returns>
        IEnumerable<T> GetList<T>(string sql, DbParameter[] parameters = null) where T : class;
        /// <summary>
        /// 异步返回一个泛型集合
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数集合</param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetListAsync<T>(string sql, DbParameter[] parameters = null) where T : class;

        /// <summary>
        /// 返回一个泛型集合
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="topNum">提取记录数</param>
        /// <param name="condition">条件</param>
        /// <param name="sort">排序语句，可多条，如 Sort asc,AutoID desc</param>
        /// <param name="filter">可选字段/列，默认是所有 *</param>
        /// <returns></returns>
        IEnumerable<T> GetList<T>(int topNum = 0, string condition = "", string sort = "", string filter = "*") where T : class;
        /// <summary>
        /// 异步返回一个泛型集合
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="topNum">提取记录数</param>
        /// <param name="condition">条件</param>
        /// <param name="sort">排序语句，可多条，如 Sort asc,AutoID desc</param>
        /// <param name="filter">可选字段/列，默认是所有 *</param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetListAsync<T>(int topNum = 0, string condition = "", string sort = "", string filter = "*") where T : class;

        #endregion

        #region 分页
        /// <summary>
        /// 获取记录数
        /// </summary>
        /// <param name="tableName">数据库表名</param>
        /// <param name="condition">条件</param>
        /// <returns></returns>
        int GetCount(string tableName, string condition = "");
        /// <summary>
        /// 异步获取记录数
        /// </summary>
        /// <param name="tableName">数据库表名</param>
        /// <param name="condition">条件</param>
        /// <returns></returns>
        Task<int> GetCountAsync(string tableName, string condition = "");

        /// <summary>
        /// 获取记录数
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="condition">条件</param>
        /// <returns></returns>
        int GetCount<T>(string condition = "");
        /// <summary>
        /// 异步获取记录数
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="condition">条件</param>
        /// <returns></returns>
        Task<int> GetCountAsync<T>(string condition = "");

        /// <summary>
        /// 分页返回一个数据表
        /// </summary>
        /// <param name="tableName">数据库表名</param>
        /// <param name="condition">条件</param>
        /// <param name="sort">排序语句，可多条，如 Sort asc,AutoID desc</param>
        /// <param name="pageIndex">页号</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="filter">可选字段/列，默认是所有 *</param>
        /// <returns></returns>
        DataTable GetPagerDT(string tableName, string condition, string sort, int pageIndex, int pageSize, string filter = "*");

        /// <summary>
        /// 分页，并返回总记录数，总页数
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="sort">排序语句，可多条，如 Sort asc,AutoID desc</param>
        /// <param name="pageIndex">页号</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="filter">可选字段/列，默认是所有 *</param>
        /// <returns></returns>
        IEnumerable<T> GetPagerList<T>(string condition, string sort, int pageIndex, int pageSize, string filter = "*") where T : class;
        /// <summary>
        /// 异步分页
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="sort">排序语句，可多条，如 Sort asc,AutoID desc</param>
        /// <param name="pageIndex">页号</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="filter">可选字段/列，默认是所有 *</param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetPagerListAsync<T>(string condition, string sort, int pageIndex, int pageSize, string filter = "*") where T : class;

        #endregion

        #endregion

        #region ------插入操作------

        /// <summary>
        /// 插入到数据库表并返回主键值（默认为自增主键）
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="model">对象实例</param>
        /// <returns></returns>
        int InsertModel<T>(T model) where T : class;
        /// <summary>
        /// 异步插入到数据库表并返回主键值（默认为自增主键）
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="model">对象实例</param>
        /// <returns></returns>
        Task<int> InsertModelAsync<T>(T model) where T : class;

        /// <summary>
        /// 插入到指定表并返回主键值
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="model">对象实例</param>
        /// <param name="tableName">数据库表名</param>
        /// <returns></returns>
        int InsertModel<T>(T model, string tableName) where T : class;
        /// <summary>
        /// 异步插入到指定表并返回主键值
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="model">对象实例</param>
        /// <param name="tableName">数据库表名</param>
        /// <returns></returns>
        Task<int> InsertModelAsync<T>(T model, string tableName) where T : class;

        #endregion

        #region ------更新数据------

        /// <summary>
        /// 更新数据表
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="model">对象实例</param>
        /// <param name="condition">指定的条件（不提供则按按主键更新）,优先</param>
        /// <returns></returns>
        bool UpdateModel<T>(T model, string condition = "") where T : class;
        /// <summary>
        /// 异步更新数据表
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="model">对象实例</param>
        /// <param name="condition">指定的条件（不提供则按按主键更新）,优先</param>
        /// <returns></returns>
        Task<bool> UpdateModelAsync<T>(T model, string condition = "") where T : class;

        #endregion

        #region ------删除数据------

        /// <summary>
        /// 从数据库表中删除数据
        /// </summary>
        /// <param name="keyValue">主键值</param>
        /// <returns></returns>
        bool Delete<T>(object keyValue);
        /// <summary>
        /// 异步从数据库表中删除数据
        /// </summary>
        /// <param name="keyValue">主键值</param>
        /// <returns></returns>
        Task<bool> DeleteAsync<T>(object keyValue);

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="model">对象实例</param>
        /// <returns></returns>
        bool DeleteModel<T>(T model) where T : class;
        /// <summary>
        /// 异步删除实体
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="model">对象实例</param>
        /// <returns></returns>
        Task<bool> DeleteModelAsync<T>(T model) where T : class;

        #endregion

        #region ------执行事务------

        /// <summary>
        /// 执行事务
        /// </summary>
        /// <param name="lstSql">多条sql语句</param>
        void ExecTrans(IEnumerable<string> lstSql);

        /// <summary>
        /// 异步执行事务
        /// </summary>
        /// <param name="lstSql"></param>
        /// <returns></returns>
        Task ExecTransAsync(IEnumerable<string> lstSql);

        /// <summary>
        /// 执行事务
        /// </summary>
        /// <param name="lstSql">多条sql语句</param>
        void ExecTrans(IDictionary<string, DbParameter[]> lstSql);

        /// <summary>
        /// 异步执行事务
        /// </summary>
        /// <param name="lstSql">多条sql语句</param>
        /// <returns></returns>
        Task ExecTransAsync(IDictionary<string, DbParameter[]> lstSql);

        #endregion        

        #region ------执行存储过程------

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="arrParams"></param>
        /// <returns></returns>
        bool ExecProc(string commandText, DbParameter[] arrParams);
        /// <summary>
        /// 执行存储过程并返回值
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="arrParams"></param>
        /// <returns></returns>
        object ExecProcReValue(string commandText, DbParameter[] arrParams);
        /// <summary>
        /// 执行存储过程并返回datareader
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="arrParams"></param>
        /// <returns></returns>
        IDataReader ExecProcReReader(string commandText, DbParameter[] arrParams);
        /// <summary>
        /// 返回一个dataset
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="arrParams"></param>
        /// <returns></returns>
        DataSet ExecProcReDS(string commandText, DbParameter[] arrParams);
        /// <summary>
        /// 返回一个datatable
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="arrParams"></param>
        /// <returns></returns>
        DataTable ExecProcReDT(string commandText, DbParameter[] arrParams);

        #endregion
    }
}
