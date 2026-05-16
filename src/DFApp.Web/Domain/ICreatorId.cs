using System;

namespace DFApp.Web.Domain;

/// <summary>
/// 创建者 ID 接口
/// </summary>
public interface ICreatorId
{
    /// <summary>
    /// 创建者 ID
    /// </summary>
    Guid? CreatorId { get; set; }
}
