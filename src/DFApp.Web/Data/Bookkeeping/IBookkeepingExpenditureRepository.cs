using DFApp.Bookkeeping;
using DFApp.Web.Data;

namespace DFApp.Web.Data.Bookkeeping
{
    /// <summary>
    /// 记账支出仓储接口
    /// </summary>
    public interface IBookkeepingExpenditureRepository : ISqlSugarRepository<BookkeepingExpenditure, long>
    {
    }
}
