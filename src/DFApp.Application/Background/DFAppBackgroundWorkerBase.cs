using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;

namespace DFApp.Background
{
    public class DFAppBackgroundWorkerBase : BackgroundWorkerBase, IDFAppBackgroundWorkerBase
    {
        protected Task? _executeTask;

        public virtual Task? ExecuteTask { get { return _executeTask; } }


        public DFAppBackgroundWorkerBase()
        {
            
        }



    }
}
