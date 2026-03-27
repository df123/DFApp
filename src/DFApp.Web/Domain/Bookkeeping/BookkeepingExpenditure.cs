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
    /// 记账支出实体
    /// </summary>
    [SugarTable("BookkeepingExpenditures")]
    public class BookkeepingExpenditure : AuditedEntity<long>
    {
        /// <summary>
        /// 支出日期
        /// </summary>
        [SugarColumn(ColumnDataType = "Date")]
        public DateTime ExpenditureDate { get; set; }

        /// <summary>
        /// 支出金额
        /// </summary>
        public decimal Expenditure { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 是否属于自己
        /// </summary>
        public bool IsBelongToSelf { get; set; }

        /// <summary>
        /// 分类
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public BookkeepingCategory? Category { get; set; }

        /// <summary>
        /// 分类ID
        /// </summary>
        public long CategoryId { get; set; }
    }
}
