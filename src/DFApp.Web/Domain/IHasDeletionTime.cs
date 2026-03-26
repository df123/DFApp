using System;

namespace DFApp.Web.Domain;

/// <summary>
/// 删除时间接口
/// </summary>
public interface IHasDeletionTime
{
    /// <summary>
    /// 删除时间
    /// </summary>
    DateTime? DeletionTime { get; set; }
}
