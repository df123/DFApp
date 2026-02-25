# RSS订阅功能文档

## 目录
- [功能概述](#功能概述)
- [技术架构](#技术架构)
- [数据库设计](#数据库设计)
- [后端文件结构](#后端文件结构)
- [API接口文档](#api接口文档)
- [核心服务说明](#核心服务说明)
- [权限定义](#权限定义)
- [使用流程](#使用流程)
- [故障排查](#故障排查)

---

## 功能概述

RSS订阅功能允许用户根据关键词、质量、字幕组等条件自动匹配RSS镜像条目，并自动添加到Aria2下载队列。主要功能包括：

### 核心功能
1. **订阅规则管理** - 创建、编辑、删除订阅规则
2. **关键词匹配** - 支持多个关键词，逗号分隔
3. **多条件过滤** - 支持做种者、下载者、完成数、质量、字幕组等过滤
4. **自动下载** - 匹配成功后自动添加到Aria2
5. **下载状态追踪** - 记录下载任务状态（待下载、下载中、完成、失败）
6. **Aria2集成** - 自动下载完成状态同步

### 业务价值
- 自动化番剧/资源订阅
- 智能过滤，只下载符合条件的内容
- 完整的下载历史记录
- 无需手动操作，全自动下载

---

## 技术架构

### 技术栈
- **框架**: ASP.NET Core 10.0
- **ORM**: Entity Framework Core
- **数据库**: SQLite
- **后台任务**: Quartz.NET
- **依赖注入**: Microsoft.Extensions.DependencyInjection
- **日志**: ILogger
- **对象映射**: Riok.Mapperly (源生成器)

### 架构模式
采用ABP Framework的DDD分层架构：

```
DFApp.Domain (领域层)
  ├── Entities (实体)
  └── Services (领域服务接口)

DFApp.Application (应用层)
  ├── AppServices (应用服务)
  ├── Background Workers (后台任务)
  └── DTOs (数据传输对象)

DFApp.EntityFrameworkCore (数据访问层)
  ├── DbContext (数据库上下文)
  └── Entity Configurations (实体配置)

DFApp.Application.Contracts (契约层)
  ├── Interfaces (服务接口)
  ├── DTOs (DTO定义)
  └── Permissions (权限定义)

DFApp.HttpApi (HTTP API层)
  └── 自动生成的HTTP API控制器

DFApp.Web (Web层)
  └── 托管前端静态文件
```

---

## 数据库设计

### 表结构

#### 1. AppRssSubscriptions (订阅规则表)
```sql
CREATE TABLE "AppRssSubscriptions" (
    "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,                          -- 订阅名称
    "Keywords" TEXT NOT NULL,                       -- 关键词（逗号分隔）
    "IsEnabled" INTEGER NOT NULL DEFAULT 1,        -- 是否启用
    "MinSeeders" INTEGER,                          -- 最小做种者数量
    "MaxSeeders" INTEGER,                          -- 最大做种者数量
    "MinLeechers" INTEGER,                         -- 最小下载者数量
    "MaxLeechers" INTEGER,                         -- 最大下载者数量
    "MinDownloads" INTEGER,                         -- 最小完成下载数
    "MaxDownloads" INTEGER,                         -- 最大完成下载数
    "QualityFilter" TEXT,                          -- 质量过滤（如：1080p,4K）
    "SubtitleGroupFilter" TEXT,                     -- 字幕组过滤（逗号分隔）
    "AutoDownload" INTEGER NOT NULL DEFAULT 1,      -- 是否自动下载
    "VideoOnly" INTEGER NOT NULL DEFAULT 0,         -- 是否仅下载视频
    "EnableKeywordFilter" INTEGER NOT NULL DEFAULT 0, -- 是否启用关键词过滤
    "SavePath" TEXT,                              -- 自定义保存路径
    "RssSourceId" INTEGER,                         -- 限制的RSS源ID
    "StartDate" TEXT,                              -- 有效期开始
    "EndDate" TEXT,                                -- 有效期结束
    "Remark" TEXT,                                -- 备注
    "CreationTime" TEXT NOT NULL,                   -- 创建时间
    "LastModificationTime" TEXT,                    -- 最后修改时间
    "ConcurrencyStamp" TEXT NOT NULL DEFAULT '',     -- 并发标记
    "CreatorId" TEXT,                             -- 创建者ID
    FOREIGN KEY ("RssSourceId") REFERENCES "AppRssSources" ("Id")
);

CREATE INDEX "IX_AppRssSubscriptions_IsEnabled" ON "AppRssSubscriptions" ("IsEnabled");
CREATE INDEX "IX_AppRssSubscriptions_RssSourceId" ON "AppRssSubscriptions" ("RssSourceId");
CREATE INDEX "IX_AppRssSubscriptions_CreationTime" ON "AppRssSubscriptions" ("CreationTime" DESC);
```

#### 2. AppRssSubscriptionDownloads (订阅下载记录表)
```sql
CREATE TABLE "AppRssSubscriptionDownloads" (
    "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "SubscriptionId" INTEGER NOT NULL,              -- 订阅ID
    "RssMirrorItemId" INTEGER NOT NULL,             -- RSS镜像条目ID
    "Aria2Gid" TEXT NOT NULL,                     -- Aria2任务GID
    "DownloadStatus" INTEGER NOT NULL DEFAULT 0,     -- 下载状态（0=待下载,1=下载中,2=完成,3=失败）
    "ErrorMessage" TEXT,                            -- 错误信息
    "DownloadStartTime" TEXT,                       -- 开始下载时间
    "DownloadCompleteTime" TEXT,                    -- 完成下载时间
    "CreationTime" TEXT NOT NULL,                   -- 创建时间
    "CreatorId" TEXT,                              -- 创建者ID
    FOREIGN KEY ("SubscriptionId") REFERENCES "AppRssSubscriptions" ("Id") ON DELETE CASCADE,
    FOREIGN KEY ("RssMirrorItemId") REFERENCES "AppRssMirrorItems" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AppRssSubscriptionDownloads_SubscriptionId" ON "AppRssSubscriptionDownloads" ("SubscriptionId");
CREATE INDEX "IX_AppRssSubscriptionDownloads_RssMirrorItemId" ON "AppRssSubscriptionDownloads" ("RssMirrorItemId");
CREATE INDEX "IX_AppRssSubscriptionDownloads_DownloadStatus" ON "AppRssSubscriptionDownloads" ("DownloadStatus");
CREATE INDEX "IX_AppRssSubscriptionDownloads_Aria2Gid" ON "AppRssSubscriptionDownloads" ("Aria2Gid");
```

### 关系说明
- 一个订阅规则可以有多个下载记录（1:N）
- 一个下载记录关联一个订阅规则和一个RSS镜像条目（N:1）
- 级联删除：删除订阅规则时，自动删除相关的下载记录
- 级联删除：删除RSS镜像条目时，自动删除相关的下载记录

---

## 后端文件结构

### Domain层

#### `/src/DFApp.Domain/Rss/`

**RssSubscription.cs**
```csharp
/// <summary>
/// RSS订阅实体
/// </summary>
public class RssSubscription : Entity<long>, IHasCreationTime, IHasModificationTime, IHasConcurrencyStamp
{
    public string Name { get; set; }                    // 订阅名称
    public string Keywords { get; set; }                 // 关键词
    public bool IsEnabled { get; set; }                  // 是否启用
    public int? MinSeeders { get; set; }                // 最小做种者
    public int? MaxSeeders { get; set; }                // 最大做种者
    public int? MinLeechers { get; set; }               // 最小下载者
    public int? MaxLeechers { get; set; }               // 最大下载者
    public int? MinDownloads { get; set; }              // 最小下载数
    public int? MaxDownloads { get; set; }              // 最大下载数
    public string? QualityFilter { get; set; }          // 质量过滤
    public string? SubtitleGroupFilter { get; set; }     // 字幕组过滤
    public bool AutoDownload { get; set; }              // 自动下载
    public bool VideoOnly { get; set; }                 // 仅视频
    public bool EnableKeywordFilter { get; set; }        // 启用关键词过滤
    public string? SavePath { get; set; }               // 保存路径
    public long? RssSourceId { get; set; }              // RSS源ID
    public DateTime? StartDate { get; set; }            // 开始日期
    public DateTime? EndDate { get; set; }              // 结束日期
    public string? Remark { get; set; }                 // 备注
    // ... 其他字段
}
```

**RssSubscriptionDownload.cs**
```csharp
/// <summary>
/// RSS订阅下载记录实体
/// </summary>
public class RssSubscriptionDownload : Entity<long>, IHasCreationTime
{
    public long SubscriptionId { get; set; }             // 订阅ID
    public long RssMirrorItemId { get; set; }           // 镜像条目ID
    public string Aria2Gid { get; set; }                // Aria2 GID
    public int DownloadStatus { get; set; }              // 下载状态
    public string? ErrorMessage { get; set; }            // 错误信息
    public DateTime? DownloadStartTime { get; set; }      // 开始时间
    public DateTime? DownloadCompleteTime { get; set; }   // 完成时间
    // ... 其他字段
}
```

**IRssSubscriptionService.cs**
```csharp
/// <summary>
/// RSS订阅服务接口
/// </summary>
public interface IRssSubscriptionService
{
    /// <summary>
    /// 检查RSS条目是否匹配订阅
    /// </summary>
    Task<List<RssSubscriptionMatchResult>> MatchSubscriptionsAsync(RssMirrorItem item);
    
    /// <summary>
    /// 从订阅规则创建下载任务
    /// </summary>
    Task CreateDownloadTaskAsync(long subscriptionId, long rssMirrorItemId);
}

public class RssSubscriptionMatchResult
{
    public long SubscriptionId { get; set; }
    public string SubscriptionName { get; set; }
    public bool Matched { get; set; }
    public string? MatchReason { get; set; }
}
```

### Application层

#### `/src/DFApp.Application/Rss/`

**RssSubscriptionService.cs**
- 实现订阅匹配逻辑
- 创建自动下载任务
- 处理多条件过滤

**RssSubscriptionAppService.cs**
- 订阅CRUD操作
- 启用/禁用订阅
- 分页查询

**RssSubscriptionDownloadAppService.cs**
- 下载记录查询
- 下载状态更新
- 重试失败任务

#### `/src/DFApp.Application/Background/`

**RssMirrorFetchWorker.cs (修改)**
- 在抓取RSS后自动匹配订阅
- 为匹配的订阅创建下载任务

**Aria2BackgroundWorker.cs (修改)**
- 监听Aria2下载完成通知
- 自动更新订阅下载记录状态

### EntityFrameworkCore层

**DFAppDbContext.cs**
添加了：
```csharp
public DbSet<RssSubscription> RssSubscriptions { get; set; }
public DbSet<RssSubscriptionDownload> RssSubscriptionDownloads { get; set; }
```

### Application.Contracts层

#### `/src/DFApp.Application.Contracts/Rss/`

**DTO定义**：
- `RssSubscriptionDto` - 订阅数据传输对象
- `CreateUpdateRssSubscriptionDto` - 创建/更新订阅DTO
- `RssSubscriptionDownloadDto` - 下载记录DTO
- `GetRssSubscriptionsRequestDto` - 查询请求DTO
- `GetRssSubscriptionDownloadsRequestDto` - 下载记录查询请求DTO

**服务接口**：
- `IRssSubscriptionAppService` - 订阅管理接口
- `IRssSubscriptionDownloadAppService` - 下载记录管理接口

---

## API接口文档

### 订阅管理 API

#### 1. 获取订阅列表
```http
GET /api/app/rss-subscription
```

**Query参数**:
- `skipCount`: 跳过条数（分页）
- `maxResultCount`: 最大返回条数（分页）
- `sorting`: 排序（例如：`"CreationTime desc"`）
- `filter`: 过滤关键词（搜索名称或关键词）
- `isEnabled`: 是否启用筛选
- `rssSourceId`: RSS源ID筛选

**响应**:
```json
{
  "totalCount": 10,
  "items": [
    {
      "id": 1,
      "name": "葬送的芙莉莲",
      "keywords": "葬送的芙莉莲,Sousou no Frieren",
      "isEnabled": true,
      "autoDownload": true,
      "videoOnly": true,
      "qualityFilter": "1080p",
      "rssSourceId": 1,
      "rssSourceName": "Sukebei Nyaa",
      "creationTime": "2026-02-24T10:00:00Z"
    }
  ]
}
```

#### 2. 获取订阅详情
```http
GET /api/app/rss-subscription/{id}
```

**响应**: 单个订阅对象

#### 3. 创建订阅
```http
POST /api/app/rss-subscription
```

**请求体**:
```json
{
  "name": "葬送的芙莉莲",
  "keywords": "葬送的芙莉莲,Sousou no Frieren",
  "isEnabled": true,
  "autoDownload": true,
  "videoOnly": true,
  "qualityFilter": "1080p",
  "subtitleGroupFilter": "字幕组A,字幕组B",
  "minSeeders": 10,
  "rssSourceId": 1,
  "remark": "第一季"
}
```

**权限**: `DFApp.RssSubscription.Create`

#### 4. 更新订阅
```http
PUT /api/app/rss-subscription/{id}
```

**请求体**: 同创建订阅

**权限**: `DFApp.RssSubscription.Update`

#### 5. 删除订阅
```http
DELETE /api/app/rss-subscription/{id}
```

**权限**: `DFApp.RssSubscription.Delete`

#### 6. 启用/禁用订阅
```http
POST /api/app/rss-subscription/{id}/toggle-enable
```

**权限**: `DFApp.RssSubscription.Update`

---

### 下载记录管理 API

#### 1. 获取下载记录列表
```http
GET /api/app/rss-subscription-download
```

**Query参数**:
- `skipCount`: 跳过条数（分页）
- `maxResultCount`: 最大返回条数（分页）
- `sorting`: 排序
- `subscriptionId`: 订阅ID筛选
- `rssMirrorItemId`: 镜像条目ID筛选
- `downloadStatus`: 下载状态筛选（0=待下载,1=下载中,2=完成,3=失败）
- `filter`: 关键词搜索
- `startTime`: 开始时间（ISO 8601）
- `endTime`: 结束时间（ISO 8601）

**响应**:
```json
{
  "totalCount": 100,
  "items": [
    {
      "id": 1,
      "subscriptionId": 1,
      "subscriptionName": "葬送的芙莉莲",
      "rssMirrorItemId": 100,
      "rssMirrorItemTitle": "[Example] 葬送的芙莉莲 第01话 [1080p]",
      "rssMirrorItemLink": "https://example.com/download/123",
      "rssSourceName": "Sukebei Nyaa",
      "aria2Gid": "abc123",
      "downloadStatus": 2,
      "downloadStatusText": "下载完成",
      "downloadStartTime": "2026-02-24T10:00:00Z",
      "downloadCompleteTime": "2026-02-24T10:30:00Z",
      "creationTime": "2026-02-24T10:00:00Z"
    }
  ]
}
```

#### 2. 获取下载记录详情
```http
GET /api/app/rss-subscription-download/{id}
```

#### 3. 删除下载记录
```http
DELETE /api/app/rss-subscription-download/{id}
```

**权限**: `DFApp.RssSubscription.Delete`

#### 4. 批量删除下载记录
```http
DELETE /api/app/rss-subscription-download/many
```

**请求体**: `[1, 2, 3]` (ID数组)

**权限**: `DFApp.RssSubscription.Delete`

#### 5. 清空所有下载记录
```http
POST /api/app/rss-subscription-download/clear-all
```

#### 6. 重试下载
```http
POST /api/app/rss-subscription-download/{id}/retry
```

**说明**: 仅允许重试失败的下载任务

**权限**: `DFApp.RssSubscription.Default`

---

## 核心服务说明

### 1. 订阅匹配服务 (RssSubscriptionService)

#### 匹配流程
```csharp
public async Task<List<RssSubscriptionMatchResult>> MatchSubscriptionsAsync(RssMirrorItem item)
{
    // 1. 获取所有启用的订阅
    var enabledSubscriptions = await _rssSubscriptionRepository.GetListAsync(s => s.IsEnabled);
    
    // 2. 遍历每个订阅进行匹配
    foreach (var subscription in enabledSubscriptions)
    {
        // 3. 检查RSS源限制
        if (subscription.RssSourceId.HasValue && subscription.RssSourceId != item.RssSourceId)
            continue;
        
        // 4. 检查时间限制
        if (subscription.StartDate.HasValue && item.PublishDate < subscription.StartDate)
            continue;
        if (subscription.EndDate.HasValue && item.PublishDate > subscription.EndDate)
            continue;
        
        // 5. 关键词匹配（支持多个，逗号分隔）
        var keywords = subscription.Keywords.Split(',', StringSplitOptions.RemoveEmptyEntries);
        bool keywordMatched = keywords.Any(k => 
            item.Title.Contains(k.Trim(), StringComparison.OrdinalIgnoreCase));
        if (!keywordMatched)
            continue;
        
        // 6. 质量过滤
        if (!string.IsNullOrEmpty(subscription.QualityFilter) && 
            !item.Title.Contains(subscription.QualityFilter, StringComparison.OrdinalIgnoreCase))
            continue;
        
        // 7. 字幕组过滤
        if (!string.IsNullOrEmpty(subscription.SubtitleGroupFilter))
        {
            var groups = subscription.SubtitleGroupFilter.Split(',', StringSplitOptions.RemoveEmptyEntries);
            bool groupMatched = groups.Any(g => 
                item.Title.Contains(g.Trim(), StringComparison.OrdinalIgnoreCase));
            if (!groupMatched)
                continue;
        }
        
        // 8. 数值过滤（做种者、下载者、完成数）
        if (subscription.MinSeeders.HasValue && (!item.Seeders.HasValue || item.Seeders < subscription.MinSeeders))
            continue;
        // ... 其他数值过滤
        
        // 9. 匹配成功
        result.Matched = true;
        result.MatchReason = "匹配成功";
    }
    
    return results;
}
```

#### 创建下载任务
```csharp
public async Task CreateDownloadTaskAsync(long subscriptionId, long rssMirrorItemId)
{
    // 1. 获取订阅和条目
    var subscription = await _rssSubscriptionRepository.GetAsync(subscriptionId);
    var item = await _rssMirrorItemRepository.GetAsync(rssMirrorItemId);
    
    // 2. 检查是否已下载
    var existingDownload = await _rssSubscriptionDownloadRepository.FirstOrDefaultAsync(
        d => d.SubscriptionId == subscriptionId && d.RssMirrorItemId == rssMirrorItemId);
    if (existingDownload != null)
        return;
    
    // 3. 创建Aria2下载请求
    var downloadRequest = new AddDownloadRequestDto
    {
        Urls = new List<string> { item.Link },
        VideoOnly = subscription.VideoOnly,
        EnableKeywordFilter = subscription.EnableKeywordFilter,
        SavePath = subscription.SavePath
    };
    
    // 4. 添加到Aria2
    var result = await _aria2Service.AddDownloadAsync(downloadRequest);
    
    // 5. 创建下载记录
    var downloadRecord = new RssSubscriptionDownload
    {
        SubscriptionId = subscriptionId,
        RssMirrorItemId = rssMirrorItemId,
        Aria2Gid = result.Id,
        DownloadStatus = 1, // 下载中
        DownloadStartTime = DateTime.Now,
        CreationTime = DateTime.Now
    };
    
    await _rssSubscriptionDownloadRepository.InsertAsync(downloadRecord);
    
    Logger.LogInformation("订阅 {SubscriptionName} 自动下载: {Title} (GID: {Gid})", 
        subscription.Name, item.Title, result.Id);
}
```

### 2. 后台任务集成

#### RssMirrorFetchWorker - 订阅匹配
在抓取RSS并插入新条目后，自动触发订阅匹配：
```csharp
private async Task ProcessSubscriptionsAsync(List<RssMirrorItem> newItems)
{
    // 1. 获取所有启用的订阅
    var enabledSubscriptions = await _rssSubscriptionRepository.GetListAsync(s => s.IsEnabled);
    if (!enabledSubscriptions.Any())
        return;
    
    // 2. 遍历每个新条目
    foreach (var item in newItems)
    {
        // 3. 匹配订阅
        var matchResults = await _rssSubscriptionService.MatchSubscriptionsAsync(item);
        
        // 4. 为匹配成功的订阅创建下载任务
        foreach (var matchResult in matchResults.Where(r => r.Matched))
        {
            var subscription = enabledSubscriptions.FirstOrDefault(s => s.Id == matchResult.SubscriptionId);
            if (subscription != null && subscription.AutoDownload)
            {
                await _rssSubscriptionService.CreateDownloadTaskAsync(
                    matchResult.SubscriptionId, item.Id);
                Logger.LogInformation("订阅 {SubscriptionName} 匹配并自动下载: {Title}", 
                    subscription.Name, item.Title);
            }
        }
    }
}
```

#### Aria2BackgroundWorker - 下载完成同步
监听Aria2的`aria2.onDownloadComplete`通知：
```csharp
private async Task UpdateDownloadRecordStatusAsync(Aria2NotificationDto notification)
{
    using var scope = _serviceScopeFactory.CreateScope();
    var repository = scope.ServiceProvider.GetRequiredService<IRepository<RssSubscriptionDownload, long>>();
    
    // 1. 提取GID
    if (notification.Params != null && notification.Params.Count > 0)
    {
        var gid = notification.Params[0].ToString();
        if (!string.IsNullOrEmpty(gid))
        {
            // 2. 查找相关的下载记录
            var downloads = await repository.GetListAsync(d => d.Aria2Gid == gid);
            
            // 3. 更新状态为完成
            foreach (var download in downloads)
            {
                download.DownloadStatus = 2; // 下载完成
                download.DownloadCompleteTime = DateTime.Now;
                await repository.UpdateAsync(download);
            }
            
            Logger.LogInformation("更新订阅下载记录状态: {Gid} -> 完成", gid);
        }
    }
}
```

---

## 权限定义

**文件**: `/src/DFApp.Application.Contracts/Permissions/DFAppPermissions.cs`

```csharp
public static class RssSubscription
{
    public const string Default = GroupName + ".RssSubscription";
    public const string Create = Default + ".Create";
    public const string Update = Default + ".Update";
    public const string Delete = Default + ".Delete";
    public const string Download = Default + ".Download";
}
```

**权限说明**:
- `DFApp.RssSubscription` - 基础权限，查看订阅和下载记录
- `DFApp.RssSubscription.Create` - 创建订阅
- `DFApp.RssSubscription.Update` - 更新订阅（包括启用/禁用）
- `DFApp.RssSubscription.Delete` - 删除订阅和下载记录
- `DFApp.RssSubscription.Download` - 下载订阅内容（重试失败任务）

---

## 使用流程

### 1. 创建订阅规则

**步骤**:
1. 登录系统
2. 导航到"RSS订阅管理"页面
3. 点击"新增订阅"
4. 填写配置信息：
   - **订阅名称**: 如 "葬送的芙莉莲"
   - **关键词**: 如 "葬送的芙莉莲,Sousou no Frieren"（多个关键词用逗号分隔）
   - **是否启用**: 勾选
   - **RSS源**: 可选，限制只从特定RSS源匹配
   - **质量过滤**: 如 "1080p"、"4K"
   - **字幕组过滤**: 如 "字幕组A,字幕组B"
   - **数值过滤**: 最小/最大做种者、下载者、完成数
   - **下载设置**:
     - 自动下载：勾选
     - 仅下载视频：勾选
     - 启用关键词过滤：勾选
     - 保存路径：可选
   - **有效期**: 可选，设置开始/结束日期
   - **备注**: 可选
5. 点击"确定"保存

**说明**: 保存后，后台任务会在下次RSS抓取时自动匹配并下载

### 2. 管理订阅

**启用/禁用订阅**:
- 在订阅列表中，点击"启用/禁用"按钮
- 禁用的订阅不会参与匹配

**编辑订阅**:
- 点击"编辑"按钮
- 修改订阅规则
- 保存后立即生效

**删除订阅**:
- 点击"删除"按钮
- 确认删除
- 删除后会级联删除相关的下载记录

### 3. 查看下载记录

**页面入口**: "RSS订阅管理" → "下载记录"

**筛选功能**:
- 按订阅筛选
- 按下载状态筛选（待下载、下载中、完成、失败）
- 按时间范围筛选
- 关键词搜索

**下载状态**:
- **待下载** (灰色): 匹配成功但未自动下载
- **下载中** (蓝色): 正在Aria2下载
- **下载完成** (绿色): Aria2下载完成
- **下载失败** (红色): 下载失败

### 4. 重试失败任务

**步骤**:
1. 在下载记录列表中找到失败的任务
2. 点击"重试"按钮
3. 系统会重新创建下载任务

### 5. 订阅匹配流程

1. RSS抓取任务获取新条目
2. 自动调用订阅匹配服务
3. 遍历所有启用的订阅规则
4. 检查每个订阅的过滤条件
5. 如果匹配成功且启用了自动下载，立即添加到Aria2
6. 创建下载记录，状态为"下载中"
7. Aria2下载完成后，自动更新状态为"下载完成"

---

## 故障排查

### 1. 订阅不匹配

**症状**: RSS条目已抓取但订阅未匹配

**可能原因**:
- 关键词不匹配（大小写、空格）
- RSS源限制不正确
- 超出有效期范围
- 质量过滤、字幕组过滤不匹配
- 数值条件不满足（做种者、下载者等）
- 订阅未启用

**解决方案**:
1. 检查关键词拼写和格式
2. 确认RSS源选择正确
3. 检查有效期设置
4. 放宽数值过滤条件（如最小做种者数）
5. 查看后台日志，查看匹配失败的详细原因

### 2. 自动下载未触发

**症状**: 订阅匹配成功但未自动添加到Aria2

**可能原因**:
- `AutoDownload` 设置为 false
- RssMirrorFetchWorker 未正常运行
- Aria2服务未运行
- Aria2 RPC 连接失败

**解决方案**:
1. 检查订阅的"自动下载"开关是否开启
2. 确认RssMirrorFetchWorker正常运行
3. 检查Aria2服务状态和RPC配置
4. 查看后台日志，查找错误信息

### 3. 下载状态未更新

**症状**: Aria2下载已完成但订阅下载记录状态仍为"下载中"

**可能原因**:
- Aria2BackgroundWorker 未正常运行
- WebSocket 连接断开
- GID 不匹配

**解决方案**:
1. 检查Aria2BackgroundWorker是否正常启动
2. 查看WebSocket连接状态
3. 对比Aria2Gid是否正确
4. 手动更新下载状态（通过重试功能）

### 4. 重复下载

**症状**: 同一RSS条目被多次下载

**可能原因**:
- 订阅规则重复
- 关键词过于宽泛

**解决方案**:
1. 检查是否有重复的订阅规则
2. 优化关键词，使其更加精确
3. 启用RSS源限制
4. 启用有效期限制

### 5. 性能问题

**症状**: RSS抓取变慢，订阅匹配耗时过长

**可能原因**:
- 订阅规则过多
- 过滤条件过于复杂
- 数据库查询未优化

**解决方案**:
1. 减少启用的订阅数量
2. 简化过滤条件
3. 确保数据库索引正常
4. 考虑分批处理订阅匹配

---

## 版本历史

### v1.0.0 (2026-02-24)
**初始版本**

#### 新增功能
- 订阅规则管理（CRUD）
- 多关键词匹配（逗号分隔）
- 多条件过滤（做种者、下载者、完成数、质量、字幕组）
- 自动下载到Aria2
- 下载状态追踪（4种状态）
- RSS源限制
- 有效期限制
- 下载记录查询和管理
- 重试失败任务
- Aria2下载完成自动同步

#### 技术特性
- ASP.NET Core 10.0
- ABP Framework DDD分层架构
- Quartz.NET 后台任务
- Entity Framework Core
- SQLite 数据库
- Riok.Mapperly 对象映射
- WebSocket 实时通信

#### API接口
- 6个订阅管理API
- 6个下载记录管理API

#### 权限定义
- 5个权限节点

---

## 相关资源

### 内部文档
- [ABP Framework官方文档](https://docs.abp.io/)
- [Quartz.NET文档](https://www.quartz-scheduler.net/documentation/)
- [Entity Framework Core文档](https://docs.microsoft.com/ef/core/)
- [RSS镜像功能文档](./rss-mirror-feature.md)

### 外部资源
- [RSS 2.0规范](https://www.rssboard.org/rss-specification)
- [Aria2 RPC文档](https://aria2.github.io/manual/en/html/aria2c.html#rpc-interface)

---

## 作者信息

**开发日期**: 2026-02-24
**版本**: 1.0.0
**框架**: ASP.NET Core 10.0 + ABP Framework

---

## 许可证

本功能是DFApp项目的一部分，遵循项目整体许可证。
