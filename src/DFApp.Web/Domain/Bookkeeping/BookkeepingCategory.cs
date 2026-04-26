using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.Bookkeeping
{
    /// <summary>
    /// 记账分类实体
    /// </summary>
    [SugarTable("AppBookkeepingCategory")]
    public class BookkeepingCategory : AuditedEntity<long>
    {
        /// <summary>
        /// 分类名称
        /// </summary>
        public string Category { get; set; } = null!;

        /// <summary>
        /// 支出记录集合
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<BookkeepingExpenditure> Expenditures { get; set; } = null!;
    }
}
