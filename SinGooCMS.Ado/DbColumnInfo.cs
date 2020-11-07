using System;

namespace SinGooCMS.Ado
{
    /// <summary>
    /// 数据库列信息
    /// </summary>
    public class DbColumnInfo
    {
        /// <summary>
        /// 列表
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// 数据类型
        /// </summary>
        public string DataType { get; set; } = "varchar";
        /// <summary>
        /// 长度
        /// </summary>
        public int Length { get; set; } = 0;
        /// <summary>
        /// 描述
        /// </summary>
        public string ColumnDescription { get; set; } = "";
        /// <summary>
        /// 默认值
        /// </summary>
        public string DefaultValue { get; set; }
        /// <summary>
        /// 是否自增
        /// </summary>
        public bool IsIdentity { get; set; } = false;
        /// <summary>
        /// 是否主键
        /// </summary>
        public bool IsPrimarykey { get; set; } = false;
        /// <summary>
        /// 是否可为空
        /// </summary>
        public bool IsNullable { get; set; } = true;
    }
}
