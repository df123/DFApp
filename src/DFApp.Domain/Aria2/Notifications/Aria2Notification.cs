using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFApp.Aria2.Notifications
{
    public class Aria2Notification
    {
        public string JSONRPC { get; set; }

        public string Method { get; set; }
        
        public List<ParamsItem> Params { get; set; }

    }


}
