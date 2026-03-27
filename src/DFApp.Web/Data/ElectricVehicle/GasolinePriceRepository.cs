using System;
using System.Threading.Tasks;
using DFApp.ElectricVehicle;
using DFApp.Web.Data;
using SqlSugar;

namespace DFApp.Web.Data.ElectricVehicle
{
    /// <summary>
    /// 油价仓储实现
    /// </summary>
    public class GasolinePriceRepository : SqlSugarReadOnlyRepository<GasolinePrice, Guid>, IGasolinePriceRepository
    {
        public GasolinePriceRepository(ISqlSugarClient db)
            : base(db)
        {
        }

        /// <summary>
        /// 获取指定省份的最新汽油价格
        /// </summary>
        /// <param name="province">省份</param>
        /// <returns>最新汽油价格</returns>
        public async Task<GasolinePrice?> GetLatestPriceAsync(string province)
        {
            return await GetQueryable()
                .Where(x => x.Province == province)
                .OrderByDescending(x => x.Date)
                .FirstAsync();
        }

        /// <summary>
        /// 获取指定省份和日期的汽油价格
        /// </summary>
        /// <param name="province">省份</param>
        /// <param name="date">日期</param>
        /// <returns>汽油价格</returns>
        public async Task<GasolinePrice?> GetPriceByDateAsync(string province, DateTime date)
        {
            return await GetQueryable()
                .Where(x => x.Province == province && x.Date.Date == date.Date)
                .FirstAsync();
        }
    }
}
