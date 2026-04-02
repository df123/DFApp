using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DFApp.ElectricVehicle;
using DFApp.Web.Data;
using DFApp.Web.Infrastructure;
using Microsoft.Extensions.Logging;
using IGasolinePriceRepository = DFApp.Web.Data.ElectricVehicle.IGasolinePriceRepository;
using IConfigurationInfoRepository = DFApp.Web.Data.Configuration.IConfigurationInfoRepository;

namespace DFApp.Web.Services.ElectricVehicle;

/// <summary>
/// 油价数据刷新器，从探数 API 获取最新油价数据并存入数据库
/// </summary>
public class GasolinePriceRefresher
{
    private readonly ISqlSugarRepository<GasolinePrice, Guid> _repository;
    private readonly IGasolinePriceRepository _gasolinePriceRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfigurationInfoRepository _configurationInfoRepository;
    private readonly ILogger<GasolinePriceRefresher> _logger;

    public GasolinePriceRefresher(
        ISqlSugarRepository<GasolinePrice, Guid> repository,
        IGasolinePriceRepository gasolinePriceRepository,
        IHttpClientFactory httpClientFactory,
        IConfigurationInfoRepository configurationInfoRepository,
        ILogger<GasolinePriceRefresher> logger)
    {
        _repository = repository;
        _gasolinePriceRepository = gasolinePriceRepository;
        _httpClientFactory = httpClientFactory;
        _configurationInfoRepository = configurationInfoRepository;
        _logger = logger;
    }

    /// <summary>
    /// 刷新全部省份的油价数据
    /// </summary>
    public async Task RefreshGasolinePricesAsync()
    {
        _logger.LogInformation("开始刷新油价数据（全部省份）");

        try
        {
            string apiKey;
            try
            {
                apiKey = await _configurationInfoRepository.GetConfigurationInfoValue("GasPriceApiKey", "DFApp.ElectricVehicle");
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new BusinessException("未配置油价API Key，请在系统配置中添加 GasPriceApiKey");
                }
            }
            catch
            {
                throw new BusinessException("未配置油价API Key，请在系统配置中添加 GasPriceApiKey");
            }

            var apiUrl = $"https://api.tanshuapi.com/api/youjia/v1/index?key={apiKey}";

            using var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                throw new BusinessException($"获取油价失败：HTTP {response.StatusCode}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<TanshuApiResponse>(json);

            if (apiResponse?.code != 1)
            {
                throw new BusinessException($"获取油价失败：{apiResponse?.msg}");
            }

            _logger.LogInformation("API返回数据：{Count} 条", apiResponse?.data?.list?.Count);

            var savedCount = 0;

            foreach (var item in apiResponse.data.list)
            {
                var existing = await _gasolinePriceRepository.GetPriceByDateAsync(item.province, DateTime.Parse(item.date));

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
        catch (BusinessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刷新油价失败");
            throw new BusinessException("刷新油价失败：" + ex.Message);
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

/// <summary>
/// 探数 API 响应数据结构
/// </summary>
public class TanshuApiResponse
{
    public int code { get; set; }
    public string msg { get; set; }
    public TanshuApiData data { get; set; }
}

/// <summary>
/// 探数 API 响应数据包装
/// </summary>
public class TanshuApiData
{
    public List<TanshuApiPriceItem> list { get; set; }
}

/// <summary>
/// 探数 API 油价条目
/// </summary>
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
