using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace SinGooCMS.Ado.Interface
{
    public interface ISqlServer
    {
        #region ------大数据批量插入------

        /// <summary>
        /// 大数据批量插入
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="targetTableName"></param>
        void BulkInsert(DataTable dt, string targetTableName);
        /// <summary>
        /// 大数据批量插入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        void BulkInsert<T>(DataTable dt) where T : class;

        #endregion        
    }
}
