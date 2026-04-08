namespace DFApp.Bookkeeping
{
    /// <summary>
    /// 比较类型，用于图表数据的周期对比
    /// </summary>
    public enum CompareType
    {
        /// <summary>
        /// 不比较
        /// </summary>
        None,

        /// <summary>
        /// 按天比较
        /// </summary>
        DAY,

        /// <summary>
        /// 按月比较
        /// </summary>
        MONTH,

        /// <summary>
        /// 按年比较
        /// </summary>
        YEAR
    }

    /// <summary>
    /// 数值类型，用于图表数据的展示方式
    /// </summary>
    public enum NumberType
    {
        /// <summary>
        /// 绝对值
        /// </summary>
        ABSOLUTE,

        /// <summary>
        /// 百分比
        /// </summary>
        PERCENTAGE
    }
}
