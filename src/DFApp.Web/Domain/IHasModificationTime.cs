using System;

namespace DFApp.Web.Domain;

/// <summary>
/// 修改时间接口
/// </summary>
public interface IHasModificationTime
{
    /// <summary>
    /// 最后修改时间
    /// </summary>
    DateTime? LastModificationTime { get; set; }
}
