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

        /// <summary>
        /// 验证复式投注输入
        /// </summary>
        /// <param name="dto">复式投注输入</param>
        /// <returns>验证结果</returns>
        Task<string> ValidateCompoundInput(CompoundLotteryInputDto dto);

        /// <summary>
        /// 生成双色球复式组合
        /// </summary>
        /// <param name="reds">红球列表</param>
        /// <param name="blues">蓝球列表</param>
        /// <returns>组合列表</returns>
        List<string> GenerateSSQCombinations(List<string> reds, List<string> blues);

        /// <summary>
        /// 生成快乐8复式组合
        /// </summary>
        /// <param name="numbers">号码列表</param>
        /// <param name="playType">玩法类型</param>
        /// <returns>组合列表</returns>
        List<string> GenerateKL8Combinations(List<string> numbers, LotteryKL8PlayType playType);
    }
}
