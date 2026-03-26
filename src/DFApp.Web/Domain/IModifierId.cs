using System;

namespace DFApp.Web.Domain;

/// <summary>
/// 修改者 ID 接口
/// </summary>
public interface IModifierId
{
    /// <summary>
    /// 最后修改者 ID
    /// </summary>
    Guid? LastModifierId { get; set; }
}
