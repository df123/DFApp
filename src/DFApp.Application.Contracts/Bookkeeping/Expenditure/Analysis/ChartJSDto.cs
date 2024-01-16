using System;
using System.Collections.Generic;
using System.Text;

namespace DFApp.Bookkeeping.Expenditure.Analysis
{
    public class ChartJSDto
    {
        /// <summary>
        /// 
        /// </summary>
        public List<string> labels { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<ChartJSDatasetsItemDto> datasets { get; set; }

        public decimal Total { get; set; }
        public decimal CompareTotal { get; set; }
        public decimal DifferenceTotal { get;set; }

        public ChartJSDto() { 
            labels = new List<string>();
            datasets = new List<ChartJSDatasetsItemDto>();
            Total = 0;
            CompareTotal = 0;
            DifferenceTotal = 0;
        }


    }
}
