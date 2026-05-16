namespace DFApp.ElectricVehicle
{
    /// <summary>
    /// 费用类型
    /// </summary>
    public enum CostType
    {
        /// <summary>
        /// 充电费
        /// </summary>
        Charging,

        /// <summary>
        /// 保养费
        /// </summary>
        Maintenance,

        /// <summary>
        /// 保险费
        /// </summary>
        Insurance,

        /// <summary>
        /// 其他费用
        /// </summary>
        Other = 3,

        /// <summary>
        /// 过路费
        /// </summary>
        Toll = 4,

        /// <summary>
        /// 停车费
        /// </summary>
        Parking = 5,

        /// <summary>
        /// 维修费
        /// </summary>
        Repair = 6
    }

    /// <summary>
    /// 汽油标号
    /// </summary>
    public enum GasolineGrade
    {
        /// <summary>
        /// 92号汽油
        /// </summary>
        H92 = 92,

        /// <summary>
        /// 95号汽油
        /// </summary>
        H95 = 95,

        /// <summary>
        /// 98号汽油
        /// </summary>
        H98 = 98
    }
}
