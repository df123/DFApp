using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFApp.Aria2.Response
{
    public class Result
    {
        public string DownloadSpeed { get; set; }

        public string NumActive { get; set; }

        public string NumStopped { get; set; }

        public string NumWaiting { get; set; }

        public string UploadSpeed { get; set; }
    }
}
