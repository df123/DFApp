using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFApp.ElectricVehicle;
using DFApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DFApp.ElectricVehicle
{
    [Authorize(DFAppPermissions.GasolinePrice.Default)]
    public class GasolinePriceService : ApplicationService, IGasolinePriceService
    {
        private readonly IRepository<GasolinePrice, Guid> _repository;
        private readonly ILogger<GasolinePriceService> _logger;
        private readonly GasolinePriceRefresher _gasolinePriceRefresher;

        public GasolinePriceService(
            IRepository<GasolinePrice, Guid> repository,
            ILogger<GasolinePriceService> logger,
            GasolinePriceRefresher gasolinePriceRefresher)
        {
            _repository = repository;
            _logger = logger;
            _gasolinePriceRefresher = gasolinePriceRefresher;
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

        public async Task<PagedResultDto<GasolinePriceDto>> GetListAsync(GetGasolinePricesDto input)
        {
            var queryable = await _repository.GetQueryableAsync();

            if (!string.IsNullOrWhiteSpace(input.Province))
            {
                queryable = queryable.Where(x => x.Province == input.Province);
            }

            if (input.StartDate.HasValue)
            {
                queryable = queryable.Where(x => x.Date >= input.StartDate.Value);
            }

            if (input.EndDate.HasValue)
            {
                queryable = queryable.Where(x => x.Date <= input.EndDate.Value);
            }

            queryable = queryable.OrderByDescending(x => x.Date);
            
            var totalCount = await AsyncExecuter.CountAsync(queryable);
            var items = await AsyncExecuter.ToListAsync(
                queryable.Skip(input.SkipCount).Take(input.MaxResultCount)
            );
            
            var dtos = ObjectMapper.Map<List<GasolinePrice>, List<GasolinePriceDto>>(items);
            
            return new PagedResultDto<GasolinePriceDto>(totalCount, dtos);
        }

        public async Task RefreshGasolinePricesAsync()
        {
            await _gasolinePriceRefresher.RefreshGasolinePricesAsync();
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
}
