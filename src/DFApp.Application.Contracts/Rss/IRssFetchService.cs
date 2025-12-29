using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace DFApp.Rss
{
    /// <summary>
    /// RSS获取服务接口
    /// </summary>
    public interface IRssFetchService : IApplicationService
    {
        /// <summary>
        /// 获取RSS Feed内容
        /// </summary>
        /// <param name="input">请求参数</param>
        /// <returns>获取结果</returns>
        Task<RssFetchResponseDto> FetchRssFeed(RssFetchRequestDto input);
    }
}