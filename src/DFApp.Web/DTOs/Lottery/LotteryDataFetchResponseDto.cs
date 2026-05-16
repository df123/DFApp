namespace DFApp.Web.DTOs.Lottery
{
    /// <summary>
    /// 彩票数据获取响应 DTO
    /// </summary>
    public class LotteryDataFetchResponseDto
    {
        /// <summary>
        /// 无参构造函数
        /// </summary>
        public LotteryDataFetchResponseDto()
        {
            Message = string.Empty;
        }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 响应消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 代理服务器返回的原始数据
        /// </summary>
        public LotteryInputDto? Data { get; set; }

        /// <summary>
        /// 请求 URL
        /// </summary>
        public string? RequestUrl { get; set; }

        /// <summary>
        /// HTTP 响应状态码
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// 响应耗时（毫秒）
        /// </summary>
        public long ResponseTime { get; set; }

        /// <summary>
        /// 保存到数据库的记录数
        /// </summary>
        public int SavedCount { get; set; }
    }
}
