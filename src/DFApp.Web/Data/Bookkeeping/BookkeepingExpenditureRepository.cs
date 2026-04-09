using DFApp.Bookkeeping;
using DFApp.Web.Data;
using SqlSugar;

namespace DFApp.Web.Data.Bookkeeping
{
    /// <summary>
    /// 记账支出仓储实现
    /// </summary>
    public class BookkeepingExpenditureRepository : SqlSugarRepository<BookkeepingExpenditure, long>, IBookkeepingExpenditureRepository
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="db">SqlSugar 客户端</param>
        public BookkeepingExpenditureRepository(ISqlSugarClient db) : base(db)
        {
        }
    }
}
