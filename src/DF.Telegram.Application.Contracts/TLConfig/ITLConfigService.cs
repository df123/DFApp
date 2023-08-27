using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace DF.Telegram.TLConfig
{
    public interface ITLConfigService:IApplicationService
    {
        void SetVerificationCode(string verificationCode);

        string Config(string what);
    }
}
