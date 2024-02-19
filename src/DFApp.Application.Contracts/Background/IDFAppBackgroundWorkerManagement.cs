using System;
using System.Collections.Generic;
using System.Text;

namespace DFApp.Background
{
    public interface IDFAppBackgroundWorkerManagement
    {
        public List<IDFAppBackgroundWorkerBase> BackgroundWorkers { get; set; }
    }
}
