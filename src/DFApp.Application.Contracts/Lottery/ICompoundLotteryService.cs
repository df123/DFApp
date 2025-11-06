using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFApp.Lottery
{
    /// <summary>
    /// 复式投注服务接口
    /// </summary>
    public interface ICompoundLotteryService
    {
        /// <summary>
        /// 计算复式投注组合
        /// </summary>
        /// <param name="dto">复式投注输入</param>
        /// <returns>组合结果</returns>
        Task<CompoundLotteryResultDto> CalculateCompoundCombination(CompoundLotteryInputDto dto);
    }
}
