using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace DFApp.Lottery
{
    /// <summary>
    /// 彩票数据获取请求DTO
    /// </summary>
    public class LotteryDataFetchRequestDto
    {
        /// <summary>
        /// 开始日期，格式：yyyy-MM-dd
        /// </summary>
        public string DayStart { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
        
        /// <summary>
        /// 结束日期，格式：yyyy-MM-dd
        /// </summary>
        public string DayEnd { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
        
        /// <summary>
        /// 页码，从1开始
        /// </summary>
        public int PageNo { get; set; } = 1;
        
        /// <summary>
        /// 彩票类型：ssq（双色球）或 kl8（快乐8）
        /// </summary>
        public string LotteryType { get; set; } = LotteryConst.SSQ_ENG;
        
        /// <summary>
        /// 是否保存到数据库
        /// </summary>
        public bool SaveToDatabase { get; set; } = false;
    }

    /// <summary>
    /// 彩票数据获取响应DTO
    /// </summary>
    public class LotteryDataFetchResponseDto
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// 获取到的数据
        /// </summary>
        public LotteryInputDto? Data { get; set; }
        
        /// <summary>
        /// 保存到数据库的记录数
        /// </summary>
        public int SavedCount { get; set; }
        
        /// <summary>
        /// 请求URL
        /// </summary>
        public string RequestUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// HTTP状态码
        /// </summary>
        public int StatusCode { get; set; }
        
        /// <summary>
        /// 响应时间（毫秒）
        /// </summary>
        public long ResponseTime { get; set; }
    }
}