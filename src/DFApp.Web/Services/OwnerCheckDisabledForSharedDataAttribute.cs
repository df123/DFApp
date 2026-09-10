using System;

namespace DFApp.Web.Services;

/// <summary>
/// 标注数据为全局共享（如彩票开奖、系统配置、下载队列），
/// 显式豁免对象级归属校验的服务必须附带此特性，供架构测试识别
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class OwnerCheckDisabledForSharedDataAttribute : Attribute
{
}

/// <summary>
/// 标注某个重写方法不执行归属校验的豁免原因（如接口已禁用、纯计算无数据访问），
/// 供架构测试跳过并留痕
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class OwnerCheckSuppressedAttribute : Attribute
{
    /// <summary>
    /// 豁免原因
    /// </summary>
    public string Reason { get; }

    public OwnerCheckSuppressedAttribute(string reason)
    {
        Reason = reason;
    }
}
