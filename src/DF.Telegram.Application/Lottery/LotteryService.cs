using DF.Telegram.Management;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DF.Telegram.Lottery
{
    public class LotteryService : CrudAppService<
        LotteryInfo,
        LotteryDto,
        long,
        PagedAndSortedResultRequestDto,
        CreateUpdateLotteryDto>, ILotteryService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public LotteryService(
            IRepository<LotteryInfo, long> repository
            , IHttpClientFactory httpClientFactory) : base(repository)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<LotteryInputDto?> GetLotteryResult(string dayStart,string dayEnd,int pageNo)
        {
            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Host", "www.cwl.gov.cn");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64; rv:102.0) Gecko/20100101 Firefox/102.0");
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");

            HttpResponseMessage message = await client.GetAsync($"https://www.cwl.gov.cn/cwl_admin/front/cwlkj/search/kjxx/findDrawNotice?name=ssq&issueCount=&issueStart=&issueEnd=&dayStart={dayStart}&dayEnd={dayEnd}&pageNo={pageNo}&pageSize=30&week=&systemType=PC");

            message.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<LotteryInputDto>(await message.Content.ReadAsStringAsync());

        }
    }
}
