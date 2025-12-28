namespace DFApp.FileFilter
{
    /// <summary>
    /// 匹配模式枚举
    /// </summary>
    public enum MatchMode
    {
        /// <summary>
        /// 包含关键词
        /// </summary>
        Contains = 0,

        /// <summary>
        /// 以关键词开头
        /// </summary>
        StartsWith = 1,

        /// <summary>
        /// 以关键词结尾
        /// </summary>
        EndsWith = 2,

        /// <summary>
        /// 完全匹配
        /// </summary>
        Exact = 3,

        /// <summary>
        /// 正则表达式匹配
        /// </summary>
        Regex = 4
    }

    /// <summary>
    /// 过滤类型枚举
    /// </summary>
    public enum FilterType
    {
        /// <summary>
        /// 黑名单：匹配到的文件将被过滤掉
        /// </summary>
        Blacklist = 0,

        /// <summary>
        /// 白名单：只有匹配到的文件才会被保留
        /// </summary>
        Whitelist = 1
    }
}