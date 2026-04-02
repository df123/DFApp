using System;

namespace DFApp.Web.Infrastructure;

/// <summary>
/// 当前用户接口，用于获取当前登录用户信息
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// 当前用户 ID
    /// </summary>
    Guid? Id { get; }

    /// <summary>
    /// 当前用户名
    /// </summary>
    string? UserName { get; }
}
