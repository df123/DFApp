using System.Collections.Generic;
using System.Text.Json.Serialization;
using ResultItemDtoType = DFApp.Lottery.ResultItemDto;

namespace DFApp.Web.DTOs.Lottery
{
    /// <summary>
    /// 彩票数据输入 DTO，用于反序列化代理服务器返回的彩票开奖数据
    /// </summary>
    public class LotteryInputDto
    {
        /// <summary>
        /// 无参构造函数
        /// </summary>
        public LotteryInputDto()
        {
            Result = new List<ResultItemDtoType>();
        }

        /// <summary>
        /// 数据总数
        /// </summary>
        [JsonPropertyName("total")]
        public int Total { get; set; }

        /// <summary>
        /// 当前页码
        /// </summary>
        [JsonPropertyName("pageNo")]
        public int PageNo { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        [JsonPropertyName("pageNum")]
        public int PageNum { get; set; }

        /// <summary>
        /// 每页大小
        /// </summary>
        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        /// <summary>
        /// 开奖结果列表（使用旧命名空间类型，与 LotteryMapper 的 External 映射方法签名匹配）
        /// </summary>
        [JsonPropertyName("result")]
        public List<ResultItemDtoType> Result { get; set; }
    }
}
