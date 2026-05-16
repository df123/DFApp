using System;

namespace DFApp.Web.Infrastructure;

/// <summary>
/// 当前用户实现
/// </summary>
public class CurrentUser : ICurrentUser
{
    /// <summary>
    /// 当前用户 ID
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// 当前用户名
    /// </summary>
    public string? UserName { get; set; }
}
