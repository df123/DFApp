﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace DFApp.Aria2.Response
{
    public class Aria2Response : ResponseBase
    {

        public string JSONRPC { get; set; }

    }
}
