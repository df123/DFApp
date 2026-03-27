# Phase 3.2 子任务 4：ConfigurationInfoRepository 迁移

## 迁移概述

将 `EfCoreConfigurationInfoRepository` 从 EF Core 迁移到 SqlSugar，保留业务逻辑，移除导航查询。

## 原仓储分析

### 原文件位置
- `src/DFApp.EntityFrameworkCore/Configuration/EfCoreConfigurationInfoRepository.cs`

### 业务方法
1. `GetAllParametersInModule(string moduleName)` - 获取指定模块的所有配置参数
2. `GetConfigurationInfoValue(string configurationName, string moduleName)` - 获取指定配置的值

### 业务逻辑
- `GetAllParametersInModule`：查询指定模块的所有配置，如果不存在则抛出 `UserFriendlyException("配置参数不存在")`
- `GetConfigurationInfoValue`：查询指定配置的值，支持模块为空的情况，如果配置或值不存在则抛出异常

### 导航查询
- 未使用导航查询

## 迁移决策

### 是否创建自定义仓储
**决定：创建自定义仓储**

### 原因
1. 有特定的业务逻辑（抛出特定的异常）
2. 查询逻辑比较特殊（`GetConfigurationInfoValue` 支持模块为空的情况）
3. 虽然查询操作相对简单，但业务逻辑需要封装在仓储中

## 迁移实施

### 1. 修改实体类

**文件：** `src/DFApp.Web/Domain/Configuration/ConfigurationInfo.cs`

**修改内容：**
- 将 `required` 关键字改为提供默认值，以满足 `new()` 约束

```csharp
// 修改前
public required string ModuleName { get; set; }
public required string ConfigurationName { get; set; }
public required string ConfigurationValue { get; set; }
public required string Remark { get; set; }

// 修改后
public string ModuleName { get; set; } = string.Empty;
public string ConfigurationName { get; set; } = string.Empty;
public string ConfigurationValue { get; set; } = string.Empty;
public string Remark { get; set; } = string.Empty;
```

### 2. 创建接口

**文件：** `src/DFApp.Web/Data/Configuration/IConfigurationInfoRepository.cs`

**接口定义：**
```csharp
public interface IConfigurationInfoRepository : ISqlSugarReadOnlyRepository<ConfigurationInfo, long>
{
    Task<string> GetConfigurationInfoValue(string configurationName, string moduleName);
    Task<List<ConfigurationInfo>> GetAllParametersInModule(string moduleName);
}
```

**说明：**
- 继承自 `ISqlSugarReadOnlyRepository<ConfigurationInfo, long>`（只读仓储）
- 保留原有的两个业务方法

### 3. 创建实现类

**文件：** `src/DFApp.Web/Data/Configuration/ConfigurationInfoRepository.cs`

**实现内容：**
```csharp
public class ConfigurationInfoRepository : SqlSugarReadOnlyRepository<ConfigurationInfo, long>, IConfigurationInfoRepository
{
    public ConfigurationInfoRepository(ISqlSugarClient db) : base(db)
    {
    }

    public async Task<string> GetConfigurationInfoValue(string configurationName, string moduleName)
    {
        var info = await GetFirstOrDefaultAsync(x => x.ConfigurationName == configurationName && (x.ModuleName == moduleName || x.ModuleName == string.Empty));

        if (info == null)
        {
            throw new UserFriendlyException("配置参数不存在");
        }

        if (info.ConfigurationValue == null)
        {
            throw new UserFriendlyException("配置参数值不存在");
        }

        return info.ConfigurationValue;
    }

    public async Task<List<ConfigurationInfo>> GetAllParametersInModule(string moduleName)
    {
        var infos = await GetListAsync(x => x.ModuleName == moduleName);

        if (infos == null || infos.Count <= 0)
        {
            throw new UserFriendlyException("配置参数不存在");
        }

        return infos;
    }
}
```

**说明：**
- 继承自 `SqlSugarReadOnlyRepository<ConfigurationInfo, long>`
- 使用 SqlSugar 的 LINQ 查询替代 EF Core 查询
- 保留原有的业务逻辑和异常处理

## 迁移对比

### 查询方式对比

| 原实现 (EF Core) | 新实现 (SqlSugar) |
|----------------|-------------------|
| `dbSet.Where(x => x.ModuleName == moduleName).ToList()` | `GetListAsync(x => x.ModuleName == moduleName)` |
| `dbSet.FirstOrDefault(x => ...)` | `GetFirstOrDefaultAsync(x => ...)` |

### 业务逻辑保留

✅ `GetAllParametersInModule` - 完全保留
✅ `GetConfigurationInfoValue` - 完全保留

## 编译问题

### 问题 1：`ConfigurationInfo` 不满足 `new()` 约束
**原因：** 实体类使用了 `required` 关键字

**解决方案：** 将 `required` 改为提供默认值

### 问题 2：缺少 using 指令
**原因：** 接口文件缺少必要的命名空间引用

**解决方案：** 添加 `System.Collections.Generic` 和 `System.Threading.Tasks`

### 问题 3：构造函数参数类型错误
**原因：** 使用了 `SqlSugarConfig` 而不是 `ISqlSugarClient`

**解决方案：** 修改为 `ISqlSugarClient db`

## 后续工作

### 需要更新的文件
1. `src/DFApp.Application/Configuration/ConfigurationInfoService.cs` - 更新仓储依赖注入

### 依赖注入配置
需要在 `Program.cs` 中注册新的仓储：
```csharp
services.AddScoped<IConfigurationInfoRepository, ConfigurationInfoRepository>();
```

## 总结

### 创建的文件
1. `src/DFApp.Web/Data/Configuration/IConfigurationInfoRepository.cs` - 接口
2. `src/DFApp.Web/Data/Configuration/ConfigurationInfoRepository.cs` - 实现
3. `docs/phase3.2-configuration-info-repository-migration.md` - 迁移文档

### 修改的文件
1. `src/DFApp.Web/Domain/Configuration/ConfigurationInfo.cs` - 修改实体类

### 保留的业务方法
1. ✅ `GetAllParametersInModule(string moduleName)`
2. ✅ `GetConfigurationInfoValue(string configurationName, string moduleName)`

### 导航查询处理
- 原仓储未使用导航查询，无需处理

### 迁移状态
✅ 完成
