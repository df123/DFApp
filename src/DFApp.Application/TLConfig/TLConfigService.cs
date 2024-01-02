using DFApp.Helper;
using DFApp.Queue;
using Volo.Abp.Application.Services;

namespace DFApp.TLConfig
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