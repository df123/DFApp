using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFApp.Aria2.Request
{
    public class Aria2Request
    {

        public Aria2Request(string id,string token)
        {
            Id = id;
            JSONRPC = Aria2Consts.JSONRPC;
            Params = new List<string>();
            if (!string.IsNullOrWhiteSpace(token))
            {
                Params.Add($"token:{token}");
            }
        }

        public string JSONRPC { get; set; }

        public string Method { get; set; }

        public string Id { get; private set; }

        public IList<string> Params { get; set; }
    }
}
