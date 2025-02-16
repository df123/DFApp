using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DFApp.Lottery.Simulation
{
    [Authorize(DFAppPermissions.Lottery.Default)]
    public class LotterySimulationService : CrudAppService<
        LotterySimulation,
        LotterySimulationDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateLotterySimulationDto>, ILotterySimulationService
    {
        public LotterySimulationService(IRepository<LotterySimulation, Guid> repository) : base(repository)
        {
            GetPolicyName = DFAppPermissions.Lottery.Default;
            GetListPolicyName = DFAppPermissions.Lottery.Default;
            CreatePolicyName = DFAppPermissions.Lottery.Create;
            UpdatePolicyName = DFAppPermissions.Lottery.Edit;
            DeletePolicyName = DFAppPermissions.Lottery.Delete;
        }

        /// <summary>
        /// 生成随机号码
        /// </summary>
        /// <param name="input">生成参数，包含生成组数、彩票类型和期号</param>
        /// <returns>生成的随机号码列表</returns>
        public async Task<ListResultDto<LotterySimulationDto>> GenerateRandomNumbersAsync(GenerateRandomNumbersDto input)
        {
            var random = new Random();
            var result = new List<LotterySimulation>();

            // 获取当前期号下最大的 groupId
            var maxGroupIdLinq = (await Repository.GetQueryableAsync())
                .Where(x => x.TermNumber == input.TermNumber)
                .Select(x => (int?)x.GroupId);
            var maxGroupId = await AsyncExecuter.MaxAsync(maxGroupIdLinq) ?? 0;
            
            // 新的 groupId 在最大值基础上 +1
            var groupId = maxGroupId + 1;

            for (int i = 0; i < input.Count; i++)
            {
                if (input.GameType == LotteryGameType.双色球)
                {
                    // 生成6个红球(1-33)
                    for (int j = 0; j < 6; j++)
                    {
                        result.Add(new LotterySimulation
                        {
                            GameType = input.GameType,
                            BallType = LotteryBallType.Red,
                            Number = random.Next(1, 34),
                            GroupId = groupId,
                            TermNumber = input.TermNumber
                        });
                    }
                    // 生成1个蓝球(1-16)
                    result.Add(new LotterySimulation
                    {
                        GameType = input.GameType,
                        BallType = LotteryBallType.Blue,
                        Number = random.Next(1, 17),
                        GroupId = groupId,
                        TermNumber = input.TermNumber
                    });
                }
                else if (input.GameType == LotteryGameType.快乐8)
                {
                    // 生成20个红球(1-80)
                    for (int j = 0; j < 20; j++)
                    {
                        result.Add(new LotterySimulation
                        {
                            GameType = input.GameType,
                            BallType = LotteryBallType.Red,
                            Number = random.Next(1, 81),
                            GroupId = groupId,
                            TermNumber = input.TermNumber
                        });
                    }
                }
                groupId++;
            }

            await Repository.InsertManyAsync(result);

            return new ListResultDto<LotterySimulationDto>(
                ObjectMapper.Map<List<LotterySimulation>, List<LotterySimulationDto>>(result)
            );
        }
    }
}
