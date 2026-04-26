namespace DFApp.Web.DTOs.Lottery
{
    public class CreateUpdateLotteryDto
    {
        public int IndexNo { get; set; }
        public string Number { get; set; }
        public string ColorType { get; set; }
        public string LotteryType { get; set; }
        public int GroupId { get; set; }
    }
}
