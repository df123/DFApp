using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DFApp.ElectricVehicle;
using DFApp.Permissions;
using DFApp.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DFApp.ElectricVehicle
{
    [Authorize(DFAppPermissions.GasolinePrice.Default)]
    public class GasolinePriceService : ApplicationService, IGasolinePriceService
    {
        private readonly IRepository<GasolinePrice, Guid> _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfigurationInfoRepository _configurationInfoRepository;
        private readonly ILogger<GasolinePriceService> _logger;

        public GasolinePriceService(
            IRepository<GasolinePrice, Guid> repository,
            IHttpClientFactory httpClientFactory,
            IConfigurationInfoRepository configurationInfoRepository,
            ILogger<GasolinePriceService> logger)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
            _configurationInfoRepository = configurationInfoRepository;
            _logger = logger;
        }

        public async Task<GasolinePriceDto?> GetLatestPriceAsync(string province)
        {
            var repository = (IGasolinePriceRepository)_repository;
            var price = await repository.GetLatestPriceAsync(province);
            return ObjectMapper.Map<GasolinePrice, GasolinePriceDto>(price);
        }

        public async Task<GasolinePriceDto?> GetPriceByDateAsync(string province, DateTime date)
        {
            var repository = (IGasolinePriceRepository)_repository;
            var price = await repository.GetPriceByDateAsync(province, date);
            return ObjectMapper.Map<GasolinePrice, GasolinePriceDto>(price);
        }

        public async Task<PagedResultDto<GasolinePriceDto>> GetListAsync(PagedAndSortedResultRequestDto input)
        {
            var queryable = await _repository.GetQueryableAsync();

            queryable = queryable.OrderByDescending(x => x.Date);
            
            var totalCount = await AsyncExecuter.CountAsync(queryable);
            var items = await AsyncExecuter.ToListAsync(
                queryable.Skip(input.SkipCount).Take(input.MaxResultCount)
            );
            
            var dtos = ObjectMapper.Map<List<GasolinePrice>, List<GasolinePriceDto>>(items);
            
            return new PagedResultDto<GasolinePriceDto>(totalCount, dtos);
        }

        public async Task RefreshGasolinePricesAsync(RefreshGasolinePriceDto input)
        {
            var province = input.Province ?? "山东";
            _logger.LogInformation("开始刷新油价数据，省份：{Province}", province);
            
            try
            {
                // 1. 从配置获取API Key
                string apiKey;
                try
                {
                    apiKey = await _configurationInfoRepository.GetConfigurationInfoValue("GasPriceApiKey", "DFApp.ElectricVehicle");
                    if (string.IsNullOrWhiteSpace(apiKey))
                    {
                        throw new UserFriendlyException("未配置油价API Key，请在系统配置中添加 GasPriceApiKey");
                    }
                }
                catch
                {
                    throw new UserFriendlyException("未配置油价API Key，请在系统配置中添加 GasPriceApiKey");
                }
                
                // 2. 调用API
                var apiUrl = $"https://api.tanshuapi.com/api/youjia/v1/index?key={apiKey}";
                
                using var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync(apiUrl);
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new UserFriendlyException($"获取油价失败：HTTP {response.StatusCode}");
                }
                
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<TanshuApiResponse>(json);
                
                if (apiResponse?.code != 1)
                {
                    throw new UserFriendlyException($"获取油价失败：{apiResponse?.msg}");
                }
                
                _logger.LogInformation("API返回数据：{Count} 条", apiResponse?.data?.list?.Count);
                
                // 3. 保存到数据库
                var savedCount = 0;
                foreach (var item in apiResponse.data.list)
                {
                    // 检查是否已存在
                    var repository = (IGasolinePriceRepository)_repository;
                    var existing = await repository.GetPriceByDateAsync(item.province, DateTime.Parse(item.date));
                    
                    if (existing == null)
                    {
                        var gasolinePrice = new GasolinePrice
                        {
                            Province = item.province,
                            Date = DateTime.Parse(item.date),
                            Price0H = ParseDecimal(item.price0h),
                            Price89H = ParseDecimal(item.price89h),
                            Price90H = ParseDecimal(item.price90h),
                            Price92H = ParseDecimal(item.price92h),
                            Price93H = ParseDecimal(item.price93h),
                            Price95H = ParseDecimal(item.price95h),
                            Price97H = ParseDecimal(item.price97h),
                            Price98H = ParseDecimal(item.price98h)
                        };
                        
                        await _repository.InsertAsync(gasolinePrice);
                        savedCount++;
                        _logger.LogInformation("保存油价数据：{Province} {Date}", item.province, item.date);
                    }
                    else
                    {
                        _logger.LogDebug("油价数据已存在：{Province} {Date}", item.province, item.date);
                    }
                }
                
                _logger.LogInformation("油价刷新完成，新增 {Count} 条记录", savedCount);
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刷新油价失败");
                throw new UserFriendlyException("刷新油价失败：" + ex.Message);
            }
        }

        private decimal? ParseDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            
            if (decimal.TryParse(value, out var result))
                return result;
            
            return null;
        }
    }

    public class TanshuApiResponse
    {
        public int code { get; set; }
        public string msg { get; set; }
        public TanshuApiData data { get; set; }
    }

    public class TanshuApiData
    {
        public List<TanshuApiPriceItem> list { get; set; }
    }

    public class TanshuApiPriceItem
    {
        public string date { get; set; }
        public string province { get; set; }
        [JsonPropertyName("0h")]
        public string price0h { get; set; }
        [JsonPropertyName("89h")]
        public string price89h { get; set; }
        [JsonPropertyName("90h")]
        public string price90h { get; set; }
        [JsonPropertyName("92h")]
        public string price92h { get; set; }
        [JsonPropertyName("93h")]
        public string price93h { get; set; }
        [JsonPropertyName("95h")]
        public string price95h { get; set; }
        [JsonPropertyName("97h")]
        public string price97h { get; set; }
        [JsonPropertyName("98h")]
        public string price98h { get; set; }
    }
}
