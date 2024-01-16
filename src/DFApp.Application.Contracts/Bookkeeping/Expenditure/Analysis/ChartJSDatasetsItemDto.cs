using System;
using System.Collections.Generic;
using System.Text;

namespace DFApp.Bookkeeping.Expenditure.Analysis
{
    public class ChartJSDatasetsItemDto
    {
        /// <summary>
        /// 
        /// </summary>
        public string label { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<decimal> data { get; set; }

        public ChartJSDatasetsItemDto() { 
            label = string.Empty;
            data = new List<decimal>();
        }

    }
}
