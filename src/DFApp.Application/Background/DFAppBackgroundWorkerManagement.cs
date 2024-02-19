using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFApp.Background
{
    public class DFAppBackgroundWorkerManagement : IDFAppBackgroundWorkerManagement
    {
        public List<IDFAppBackgroundWorkerBase> BackgroundWorkers { get; set; }
        public DFAppBackgroundWorkerManagement()
        {
            BackgroundWorkers = new List<IDFAppBackgroundWorkerBase>(8);
        }
    }
}
