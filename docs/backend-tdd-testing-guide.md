# 后端 TDD 测试指南

## 概述

本文档描述 DFApp 项目的测试驱动开发（TDD）实践指南。项目从 ABP Framework 迁移到轻量级 ASP.NET Core 架构后，采用 TDD 模式进行开发。

## 测试框架

### 技术栈

- **测试框架**: xUnit 2.9.3
- **Mock 框架**: Moq 4.20.72
- **断言库**: FluentAssertions 7.0.0
- **代码覆盖率**: coverlet.collector 6.0.4

### 项目结构

```
test/
└── DFApp.Web.Tests/
    ├── DFApp.Web.Tests.csproj
    ├── Domain/
    │   ├── EntityBaseTests.cs
    │   ├── AuditedEntityTests.cs
    │   └── FullAuditedEntityTests.cs
    ├── Data/
    │   └── (待添加)
    ├── Infrastructure/
    │   └── GlobalExceptionFilterTests.cs
    └── Hubs/
        └── (待添加)
```

## Phase 1 组件测试

### 1. 实体基类测试

#### EntityBaseTests

测试自定义实体基类的功能：

- ✅ 整数主键实体初始化
- ✅ 设置整数主键
- ✅ GUID 主键实体初始化
- ✅ 设置 GUID 主键
- ✅ 实体相等性（相同 ID）
- ✅ 实体不等性（不同 ID）
- ✅ 设置并发标记
- ✅ 并发标记初始化

**测试数量**: 7

#### AuditedEntityTests

测试审计实体的功能：

- ✅ 整数主键审计实体初始化
- ✅ GUID 主键审计实体初始化
- ✅ 设置创建时间
- ✅ 设置创建者 ID（Guid）
- ✅ 设置最后修改时间
- ✅ 设置最后修改者 ID（Guid）
- ✅ 设置完整审计属性

**测试数量**: 7

#### FullAuditedEntityTests

测试完整审计实体的功能：

- ✅ 整数主键完整审计实体初始化
- ✅ GUID 主键完整审计实体初始化
- ✅ 设置删除标记
- ✅ 设置删除时间
- ✅ 设置删除者 ID（Guid）
- ✅ 设置完整审计属性
- ✅ 软删除功能
- ✅ 恢复删除（软删除）

**测试数量**: 9

### 2. 基础设施测试

#### GlobalExceptionFilterTests

测试全局异常过滤器的功能：

- ✅ 处理业务异常（BusinessException）
- ✅ 处理未找到异常（NotFoundException）
- ✅ 处理验证异常（ValidationException）
- ✅ 处理未处理的异常（Exception）
- ✅ 异常已处理时仍会处理

**测试数量**: 5

## 测试结果

### Phase 1 组件测试统计

| 组件 | 测试数量 | 通过 | 失败 | 跳过 |
|--------|----------|------|--------|--------|
| EntityBase | 7 | 7 | 0 | 0 |
| AuditedEntity | 7 | 7 | 0 | 0 |
| FullAuditedEntity | 9 | 9 | 0 | 0 |
| GlobalExceptionFilter | 5 | 5 | 0 | 0 |
| **总计** | **28** | **28** | **0** | **0** |

**测试通过率**: 100% ✅

## TDD 实践指南

### 1. 测试命名规范

- 测试类命名: `{ComponentName}Tests`
- 测试方法命名: `{MethodName}_{ExpectedBehavior}`

示例：
```csharp
public class EntityBaseTests
{
    [Fact]
    public void EntityBase_WithIntKey_ShouldInitializeWithId()
    {
        // Arrange & Act & Assert
    }
}
```

### 2. 测试结构（AAA 模式）

使用 Arrange-Act-Assert 模式组织测试：

```csharp
[Fact]
public void OnException_BusinessException_ShouldReturnBadRequest()
{
    // Arrange - 准备测试数据和对象
    var exceptionContext = CreateExceptionContext(new BusinessException("业务错误"));
    var expectedMessage = "业务错误";

    // Act - 执行被测试的方法
    _filter.OnException(exceptionContext);

    // Assert - 验证结果
    exceptionContext.ExceptionHandled.Should().BeTrue();
    var result = exceptionContext.Result as ObjectResult;
    result?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
}
```

### 3. Mock 使用指南

使用 Moq 创建模拟对象：

```csharp
// 创建 Mock 对象
var environmentMock = new Mock<IWebHostEnvironment>();
environmentMock.Setup(x => x.EnvironmentName).Returns("Production");

// 使用 Mock 对象
services.AddSingleton(environmentMock.Object);
```

### 4. 断言最佳实践

使用 FluentAssertions 进行可读的断言：

```csharp
// 推荐使用 FluentAssertions
entity.Id.Should().Be(expectedId);
entity.CreationTime.Should().Be(expectedTime);

// 而不是传统的 Assert
Assert.Equal(expectedId, entity.Id);
```

### 5. 测试数据准备

为测试创建辅助方法：

```csharp
private ExceptionContext CreateExceptionContext(Exception exception)
{
    var httpContext = new DefaultHttpContext();
    var services = new ServiceCollection();
    var environmentMock = new Mock<IWebHostEnvironment>();
    environmentMock.Setup(x => x.EnvironmentName).Returns("Production");
    services.AddSingleton(environmentMock.Object);
    httpContext.RequestServices = services.BuildServiceProvider();

    var actionContext = new ActionContext(
        httpContext,
        new Microsoft.AspNetCore.Routing.RouteData(),
        new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());

    return new ExceptionContext(actionContext, new List<IFilterMetadata>())
    {
        Exception = exception
    };
}
```

## 运行测试

### 运行所有测试

```bash
# 在项目根目录
dotnet test test/DFApp.Web.Tests/DFApp.Web.Tests.csproj
```

### 运行特定测试类

```bash
# 运行 Domain 测试
dotnet test --filter "FullyQualifiedName~EntityBaseTests"

# 运行 Infrastructure 测试
dotnet test --filter "FullyQualifiedName~GlobalExceptionFilterTests"
```

### 查看代码覆盖率

```bash
# 生成代码覆盖率报告
dotnet test --collect:"XPlat Code Coverage"

# 使用 ReportGenerator 生成 HTML 报告
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coveragereport -reporttypes:Html
```

## 后续工作

### Phase 2 组件测试

在 Phase 2 迁移完成后，需要为以下组件添加测试：

1. **SqlSugar 仓储测试**
   - `ISqlSugarRepository` 测试
   - `ISqlSugarReadOnlyRepository` 测试
   - `SqlSugarRepository` 实现
   - `SqlSugarReadOnlyRepository` 实现

2. **权限系统测试**
   - `PermissionPolicyProvider` 测试
   - 权限定义测试
   - 授权处理器测试

3. **SignalR Hub 测试**
   - `Aria2Hub` 测试
   - 连接管理测试
   - 消息推送测试

4. **应用服务测试**
   - CRUD 服务基类测试
   - 各业务服务测试

### 集成测试

考虑添加集成测试项目：

- API 端点测试
- 数据库集成测试
- 完整业务流程测试

## 最佳实践

### 1. 测试隔离

- 每个测试应该独立运行
- 不依赖测试执行顺序
- 使用 `IDisposable` 清理资源

### 2. 测试覆盖率目标

- **单元测试覆盖率**: 目标 ≥ 80%
- **关键业务逻辑**: 目标 ≥ 90%
- **公共 API**: 目标 ≥ 95%

### 3. 持续集成

- 在 CI/CD 流程中自动运行测试
- 代码合并前必须通过所有测试
- 定期审查和更新测试

### 4. 文档更新

- 每次添加新功能时更新测试
- 保持测试文档与代码同步
- 记录测试决策和设计

## 故障排查

### 常见问题

#### 1. 测试失败

**问题**: 测试失败但代码看起来正确

**解决方案**:
- 检查测试环境设置（Development/Production）
- 验证 Mock 对象配置
- 检查异常继承关系

#### 2. 构建警告

**问题**: Entity Framework 版本冲突警告

**解决方案**:
- 这是已知问题，不影响测试运行
- 可以在 `.csproj` 中添加 `<NoWarn>MSB3277</NoWarn>`

#### 3. 测试运行缓慢

**问题**: 测试运行时间过长

**解决方案**:
- 使用 `[Theory]` 和 `[InlineData]` 参数化测试
- 避免重复的测试设置
- 考虑使用测试基类

## 相关文档

- [后端测试配置](./backend-testing-config.md)
- [框架迁移计划](./framework-migration-plan.md)
- [Phase 1 迁移总结](./phase1-migration-summary.md)

## 总结

Phase 1 组件的 TDD 测试框架已成功建立：

✅ 创建了单元测试项目
✅ 配置了测试依赖（xUnit、Moq、FluentAssertions）
✅ 编写了 28 个单元测试，全部通过
✅ 建立了测试目录结构
✅ 提供了 TDD 实践指南

测试框架已就绪，可以支持后续的 Phase 2 迁移和功能开发。
