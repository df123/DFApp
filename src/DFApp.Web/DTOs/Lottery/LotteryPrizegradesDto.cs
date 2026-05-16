using DFApp.Web.DTOs;

namespace DFApp.Web.DTOs.Lottery
{
    public class LotteryPrizegradesDto : AuditedEntityDto<long>
    {
        public string? Type { get; set; }

        public string? TypeNum { get; set; }

        public string? TypeMoney { get; set; }

    }
}
