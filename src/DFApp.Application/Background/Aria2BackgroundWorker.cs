using DFApp.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFApp.Background
{
    public class Aria2BackgroundWorker : DFAppBackgroundWorkerBase
    {
        public Aria2BackgroundWorker(string moduleName, IConfigurationInfoRepository configurationInfoRepository) : base(moduleName, configurationInfoRepository)
        {
        }
    }
}
