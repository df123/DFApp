using System.Reflection;
using DFApp.Web.Data;
using DFApp.Web.Domain;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;
using DFApp.Web.Services;

namespace DFApp.Web.Tests.Services;

/// <summary>
/// 归属校验架构测试：
/// 1. CrudServiceBase.RequireOwnerCheck 默认必须为 true（fail-closed）；
/// 2. 未豁免的服务重写 GetAsync/UpdateAsync/DeleteAsync 时，
///    方法体（含异步状态机）必须调用 EnsureOwnerAsync 或基类实现，防止越权回归
/// </summary>
public class OwnerCheckArchitectureTests
{
    private static readonly Assembly WebAssembly = typeof(CrudServiceBase<,,,,>).Assembly;
    private static readonly Type CrudBaseDefinition = typeof(CrudServiceBase<,,,,>);

    /// <summary>
    /// 测试专用探针实体与服务，用于读取 RequireOwnerCheck 的默认值
    /// </summary>
    private sealed class ProbeEntity : EntityBase<long>, ICreatorId
    {
        public Guid? CreatorId { get; set; }
    }

    private sealed class ProbeDto { }

    private sealed class ProbeService : CrudServiceBase<ProbeEntity, long, ProbeDto, ProbeDto, ProbeDto>
    {
        public ProbeService()
            : base(null!, null!, null!)
        {
        }
    }

    [Fact]
    public void RequireOwnerCheck默认值应为true()
    {
        var property = CrudBaseDefinition
            .MakeGenericType(typeof(ProbeEntity), typeof(long), typeof(ProbeDto), typeof(ProbeDto), typeof(ProbeDto))
            .GetProperty("RequireOwnerCheck", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        property.Should().NotBeNull();

        var service = new ProbeService();
        var value = property!.GetValue(service);
        value.Should().Be(true, "对象级归属校验必须默认开启（fail-closed），全局共享数据需显式豁免");
    }

    [Fact]
    public void 重写单实体操作的服务必须执行归属校验()
    {
        var failures = new List<string>();

        var serviceTypes = WebAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && GetCrudBaseInstantiation(t) != null)
            .ToList();

        serviceTypes.Should().NotBeEmpty("应当能找到 CrudServiceBase 的派生服务");

        foreach (var serviceType in serviceTypes)
        {
            if (serviceType.GetCustomAttribute<OwnerCheckDisabledForSharedDataAttribute>() != null)
            {
                continue;
            }

            var entity = GetCrudBaseInstantiation(serviceType)!.GetGenericArguments()[0];
            if (!typeof(ICreatorId).IsAssignableFrom(entity))
            {
                continue;
            }

            foreach (var methodName in new[] { "GetAsync", "UpdateAsync", "DeleteAsync" })
            {
                var declared = serviceType
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(m => m.Name == methodName)
                    .ToList();

                foreach (var method in declared)
                {
                    if (method.GetCustomAttribute<OwnerCheckSuppressedAttribute>() != null)
                    {
                        continue;
                    }

                    var ok = CallsAnyOf(method, resolved =>
                    {
                        var declaring = resolved.DeclaringType;
                        var isCrudBaseMember = declaring != null
                            && declaring.IsGenericType
                            && declaring.GetGenericTypeDefinition() == CrudBaseDefinition;
                        return resolved.Name == "EnsureOwnerAsync"
                               || (isCrudBaseMember && resolved.Name == methodName);
                    });

                    if (!ok)
                    {
                        failures.Add($"{serviceType.Name}.{methodName} 重写后未调用 EnsureOwnerAsync 或基类实现");
                    }
                }
            }
        }

        failures.Should().BeEmpty($"以下重写缺失对象级归属校验：{string.Join("; ", failures)}");
    }

    /// <summary>
    /// 查找类型继承链上 CrudServiceBase 的具体泛型实例（无则返回 null）
    /// </summary>
    private static Type? GetCrudBaseInstantiation(Type type)
    {
        var current = type.BaseType;
        while (current != null)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == CrudBaseDefinition)
            {
                return current;
            }

            current = current.BaseType;
        }

        return null;
    }

    /// <summary>
    /// 判断方法的 IL（含 async 状态机 MoveNext 与编译器生成的 base 调用代理）中
    /// 是否存在满足条件的 call/callvirt 调用。
    /// 逐个解析令牌而非匹配固定值：对泛型基类方法的调用编码为 MemberRef，令牌随调用点变化；
    /// async 方法中的 base.X 调用经 &lt;&gt;n__N 代理方法转发，需递归检查
    /// </summary>
    private static bool CallsAnyOf(MethodInfo method, Func<MethodBase, bool> match)
    {
        var pending = new Queue<MethodInfo>();
        pending.Enqueue(method);
        var visited = new HashSet<MethodInfo>();

        while (pending.Count > 0)
        {
            var current = pending.Dequeue();
            if (!visited.Add(current))
            {
                continue;
            }

            foreach (var il in GetMethodIls(current))
            {
                for (var i = 0; i < il.Length - 4; i++)
                {
                    // 0x28 = call, 0x6F = callvirt，后随 4 字节方法令牌
                    if (il[i] != 0x28 && il[i] != 0x6F)
                    {
                        continue;
                    }

                    var token = BitConverter.ToInt32(il, i + 1);
                    MethodBase? resolved;
                    try
                    {
                        resolved = current.Module.ResolveMethod(token);
                    }
                    catch
                    {
                        continue;
                    }

                    if (resolved == null)
                    {
                        continue;
                    }

                    if (match(resolved))
                    {
                        return true;
                    }

                    // base 调用代理方法（<>n__0 等）：继续递归其方法体
                    if (resolved is MethodInfo proxy
                        && proxy.Name.StartsWith("<>n__", StringComparison.Ordinal)
                        && proxy.DeclaringType == method.DeclaringType)
                    {
                        pending.Enqueue(proxy);
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 收集方法的 IL 字节（含 async 状态机的 MoveNext 方法）
    /// </summary>
    private static List<byte[]> GetMethodIls(MethodInfo method)
    {
        var ils = new List<byte[]>();
        if (method.GetMethodBody()?.GetILAsByteArray() is { } il)
        {
            ils.Add(il);
        }

        // async 方法体在编译器生成的状态机 MoveNext 中
        var stateMachineAttr = method.GetCustomAttributes()
            .FirstOrDefault(a => a.GetType().Name == "AsyncStateMachineAttribute");
        if (stateMachineAttr != null)
        {
            var smType = stateMachineAttr.GetType()
                .GetProperty("StateMachineType")?.GetValue(stateMachineAttr) as Type;
            var moveNext = smType?.GetMethod("MoveNext", BindingFlags.NonPublic | BindingFlags.Instance);
            if (moveNext?.GetMethodBody()?.GetILAsByteArray() is { } smIl)
            {
                ils.Add(smIl);
            }
        }

        return ils;
    }
}
