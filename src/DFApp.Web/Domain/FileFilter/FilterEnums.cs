namespace DFApp.FileFilter
{
    /// <summary>
    /// 匹配模式
    /// </summary>
    public enum MatchMode
    {
        /// <summary>
        /// 包含
        /// </summary>
        Contains,

        /// <summary>
        /// 前缀匹配
        /// </summary>
        StartsWith,

        /// <summary>
        /// 后缀匹配
        /// </summary>
        EndsWith,

        /// <summary>
        /// 精确匹配
        /// </summary>
        Exact,

        /// <summary>
        /// 正则表达式匹配
        /// </summary>
        Regex
    }

    /// <summary>
    /// 过滤类型
    /// </summary>
    public enum FilterType
    {
        /// <summary>
        /// 黑名单（命中关键词则过滤）
        /// </summary>
        Blacklist,

        /// <summary>
        /// 白名单（未命中关键词则过滤）
        /// </summary>
        Whitelist
    }
}
