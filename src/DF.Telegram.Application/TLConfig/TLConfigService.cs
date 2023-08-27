using DF.Telegram.Helper;
using DF.Telegram.Queue;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace DF.Telegram.TLConfig
{
    public class TLConfigService:ApplicationService,ITLConfigService
    {
        private readonly IQueueBase<string> _queueBase;
        public TLConfigService(IQueueBase<string> queueBase)
        {
            _queueBase = queueBase;
        }
        public void SetVerificationCode(string verificationCode)
        {
            _queueBase.AddItem(verificationCode);
        }

#nullable disable
        public string Config(string what)
        {
            string[] sections = new string[] { "Telegram", what };
            switch (what)
            {
                case "session_pathname":
                case "api_id":
                case "api_hash":
                case "phone_number": return AppsettingsHelper.app(sections);
                case "verification_code":
                    return "";
                default: return null;
            }
        }
#nullable restore
    }
}