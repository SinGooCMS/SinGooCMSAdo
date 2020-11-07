using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace SinGooCMS.Ado.Interface
{
    public interface IOracle
    {
        #region ------Oracle可选返回值 事务------

        /// <summary>
        /// 执行多条sql，并返回影响的行数或返回的值
        /// </summary>
        /// <param name="lstSql">REVALUE/NOVALUE,(sqlscripot,params)</param>
        /// <returns></returns>
        IEnumerable<int> ExecOracleTrans(Dictionary<string, (string, DbParameter[])> lstSql);

        /// <summary>
        /// 异步执行多条sql，并返回影响的行数或返回的值
        /// </summary>
        /// <param name="lstSql"></param>
        /// <returns></returns>
        Task<IEnumerable<int>> ExecOracleTransAsync(Dictionary<string, (string, DbParameter[])> lstSql);

        #endregion

        #region ------其它------

        /// <summary>
        /// 是否存在序列
        /// </summary>
        /// <param name="sequenceName">序列名，全大写</param>
        /// <returns></returns>
        bool ExisSequence(string sequenceName);

        /// <summary>
        /// 是否存在序列
        /// </summary>
        /// <param name="sequenceName">序列名，全大写</param>
        /// <returns></returns>
        Task<bool> ExisSequenceAsync(string sequenceName);

        /// <summary>
        /// 删除序列
        /// </summary>
        /// <param name="sequenceName">序列名，全大写</param>
        void DelSequence(string sequenceName);
        /// <summary>
        /// 异步删除序列
        /// </summary>
        /// <param name="sequenceName">序列名，全大写</param>
        /// <returns></returns>
        Task DelSequenceAsync(string sequenceName);

        #endregion
    }
}
