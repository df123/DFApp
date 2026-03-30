# Phase 4.3 Batch 4 - RssMirrorItemAppService 和 RssWordSegmentAppService 迁移摘要

## 迁移日期
2026-03-30

## 迁移的服务

### 1. RssMirrorItemAppService
- **原文件**: `src/DFApp.Application/Rss/RssMirrorItemAppService.cs`
- **新文件**: `src/DFApp.Web/Services/Rss/RssMirrorItemAppService.cs`

#### 迁移变更
| 变更项 | 原实现 | 新实现 |
|--------|--------|--------|
| 基类 | `ApplicationService` | `AppServiceBase` |
| 仓储 | `IRepository<T, long>` (ABP) | `ISqlSugarRepository<T, long>` (SqlSugar) |
| 权限 | `[Authorize]` 属性 | `CheckPermissionAsync()` 方法调用 |
| 异常 | `UserFriendlyException` | `BusinessException` |
| 映射 | `ObjectMapper.Map<>()` | 手动映射 + `MapToDto()` / `MapWordSegmentToDto()` |
| 分页查询 | `AsyncExecuter.ToListAsync()` | SqlSugar `ToListAsync()` |
| 子查询 | `IQueryable.Contains()` | 先 `ToListAsync()` 再 `List.Contains()` |
| 导航属性 | 直接访问 `item.Source` | 通过外键查询 `_rssSourceRepository` |

#### 方法清单（8个）
1. `GetListAsync` - 获取镜像条目分页列表
2. `GetAsync` - 根据ID获取镜像条目
3. `DeleteAsync` - 删除镜像条目（含关联分词）
4. `DeleteManyAsync` - 批量删除镜像条目
5. `GetWordSegmentStatisticsAsync` - 获取分词统计
6. `GetByWordTokenAsync` - 根据分词获取镜像条目
7. `ClearAllAsync` - 清空所有镜像数据
8. `DownloadToAria2Async` - 下载到Aria2（伪代码，IAria2Service 未迁移）

#### 待办事项
- `IAria2Service` 未迁移，`DownloadToAria2Async` 方法中使用伪代码替代
- 使用 Mapperly 替代手动映射

### 2. RssWordSegmentAppService
- **原文件**: `src/DFApp.Application/Rss/RssWordSegmentAppService.cs`
- **新文件**: `src/DFApp.Web/Services/Rss/RssWordSegmentAppService.cs`

#### 迁移变更
| 变更项 | 原实现 | 新实现 |
|--------|--------|--------|
| 基类 | `ApplicationService` | `AppServiceBase` |
| 仓储 | `IRepository<T, long>` (ABP) | `ISqlSugarRepository<T, long>` (SqlSugar) |
| 权限 | `[Authorize]` 属性 | `CheckPermissionAsync()` 方法调用 |
| 映射 | `ObjectMapper.Map<>()` | 手动映射 + `MapToDto()` |
| 分页查询 | `AsyncExecuter.ToListAsync()` | SqlSugar `ToListAsync()` |
| 子查询 | `IQueryable.Contains()` | 先 `ToListAsync()` 再 `List.Contains()` |
| 导航属性 | 直接访问 `segment.Item` | 通过外键查询 `_rssMirrorItemRepository` |

#### 方法清单（4个）
1. `GetListAsync` - 获取分词列表（分页，带关联数据）
2. `GetStatisticsAsync` - 获取分词统计（带分页）
3. `DeleteByItemAsync` - 删除指定RSS镜像条目的所有分词
4. `DeleteBySourceAsync` - 删除指定RSS源的所有分词

#### 待办事项
- 使用 Mapperly 替代手动映射

## 依赖关系
- `RssMirrorItemAppService` 依赖 `IAria2Service`（未迁移，已用伪代码标注）
- 两个服务均依赖已迁移的实体：`RssMirrorItem`、`RssWordSegment`、`RssSource`
- 使用 `DFApp.Permissions.DFAppPermissions` 权限常量（来自 Application.Contracts）
- 使用 `PagedResultDto<T>`（定义在 `DFApp.Web.Services.ElectricVehicle` 命名空间）

## 迁移模式总结
1. **仓储替换**: ABP `IRepository<T, long>` → `ISqlSugarRepository<T, long>`
2. **查询方式**: `_repository.GetQueryableAsync()` → `_repository.GetQueryable()`（同步获取）
3. **子查询处理**: `ISugarQueryable` 不支持 `.Contains()`，需先 `.ToListAsync()` 再用 `List.Contains()`
4. **权限检查**: `[Authorize]` 属性 → `CheckPermissionAsync()` 方法
5. **导航属性**: 通过外键 ID 手动查询关联实体
6. **映射**: `ObjectMapper` → 手动映射方法，标注 `// TODO: 使用 Mapperly 映射`
