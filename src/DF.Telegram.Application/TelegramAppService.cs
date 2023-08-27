using System;
using System.Collections.Generic;
using System.Text;
using DF.Telegram.Localization;
using Volo.Abp.Application.Services;

namespace DF.Telegram;

/* Inherit your application services from this class.
 */
public abstract class TelegramAppService : ApplicationService
{
    protected TelegramAppService()
    {
        LocalizationResource = typeof(TelegramResource);
    }
}
