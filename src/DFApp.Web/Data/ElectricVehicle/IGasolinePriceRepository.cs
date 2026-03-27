using System;
using System.Threading.Tasks;
using DFApp.ElectricVehicle;
using DFApp.Web.Data;

namespace DFApp.Web.Data.ElectricVehicle
{
    /// <summary>
    /// 油价仓储接口
    /// </summary>
    public interface IGasolinePriceRepository : ISqlSugarReadOnlyRepository<GasolinePrice, Guid>
    {
        /// <summary>
        /// 获取指定省份的最新汽油价格
        /// </summary>
        /// <param name="province">省份</param>
        /// <returns>最新汽油价格</returns>
        Task<GasolinePrice?> GetLatestPriceAsync(string province);

        /// <summary>
        /// 获取指定省份和日期的汽油价格
        /// </summary>
        /// <param name="province">省份</param>
        /// <param name="date">日期</param>
        /// <returns>汽油价格</returns>
        Task<GasolinePrice?> GetPriceByDateAsync(string province, DateTime date);
    }
}
