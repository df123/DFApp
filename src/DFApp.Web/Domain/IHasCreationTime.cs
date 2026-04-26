using System;

namespace DFApp.Web.Domain;

/// <summary>
/// 创建时间接口
/// </summary>
public interface IHasCreationTime
{
    /// <summary>
    /// 创建时间
    /// </summary>
    DateTime CreationTime { get; set; }
}
