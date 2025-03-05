using System.Collections.Generic;

namespace DFApp.Bookkeeping.Expenditure
{
    public class MonthlyExpenditureDto
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<decimal> TotalData { get; set; } = new List<decimal>();
        public List<decimal> SelfData { get; set; } = new List<decimal>();
        public List<decimal> NonSelfData { get; set; } = new List<decimal>();
    }
}
