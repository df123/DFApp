using System;

namespace DFApp.Web.Domain;

/// <summary>
/// 删除者 ID 接口
/// </summary>
public interface IDeleterId
{
    /// <summary>
    /// 删除者 ID
    /// </summary>
    Guid? DeleterId { get; set; }
}
