using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace DFApp.Lottery
{
    /// <summary>
    /// 彩票数据获取服务接口
    /// </summary>
    public interface ILotteryDataFetchService : IApplicationService
    {
        /// <summary>
        /// 手动获取彩票数据
        /// </summary>
        /// <param name="input">请求参数</param>
        /// <returns>获取结果</returns>
        Task<LotteryDataFetchResponseDto> FetchLotteryData(LotteryDataFetchRequestDto input);
        
        /// <summary>
        /// 获取双色球最新数据
        /// </summary>
        /// <returns>获取结果</returns>
        Task<LotteryDataFetchResponseDto> FetchSSQLatestData();
        
        /// <summary>
        /// 获取快乐8最新数据
        /// </summary>
        /// <returns>获取结果</returns>
        Task<LotteryDataFetchResponseDto> FetchKL8LatestData();
        
        /// <summary>
        /// 测试彩票数据API连接
        /// </summary>
        /// <param name="lotteryType">彩票类型</param>
        /// <returns>测试结果</returns>
        Task<LotteryDataFetchResponseDto> TestLotteryApiConnection(string lotteryType = LotteryConst.SSQ_ENG);
    }
}