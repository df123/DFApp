namespace DFApp.Lottery
{
    /// <summary>
    /// 奖级数据项 DTO（旧命名空间，用于 JSON 反序列化）
    /// </summary>
    public class PrizegradesItemDto
    {
        public string? Type { get; set; }
        public string? TypeNum { get; set; }
        public string? TypeMoney { get; set; }
    }
}
