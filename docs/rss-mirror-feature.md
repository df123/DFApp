# RSS镜像站点功能文档

## 目录
- [功能概述](#功能概述)
- [技术架构](#技术架构)
- [数据库设计](#数据库设计)
- [后端文件结构](#后端文件结构)
- [API接口文档](#api接口文档)
- [核心服务说明](#核心服务说明)
- [权限定义](#权限定义)
- [配置说明](#配置说明)
- [使用流程](#使用流程)
- [技术细节](#技术细节)
- [故障排查](#故障排查)

---

## 功能概述

RSS镜像站点是一个自动抓取、解析、存储和管理RSS Feed内容的完整系统。主要功能包括：

### 核心功能
1. **RSS源管理** - 管理多个RSS源配置，支持代理、关键词过滤
2. **自动抓取** - 基于Quartz.NET的后台定时任务，自动抓取RSS源
3. **智能分词** - 支持中文、英文、日文三种语言的自动分词
4. **镜像存储** - 将RSS条目镜像到数据库，支持查询和统计
5. **Aria2集成** - 一键下载到Aria2，支持视频过滤和关键词过滤
6. **分词统计页面** - 专门的分词统计页面，支持查看全部分词统计、搜索和精确筛选
7. **快速下载** - 在分词详情页面直接下载RSS条目链接到Aria2

### 业务价值
- 自动化内容收集和归档
- 多源聚合，统一管理
- 智能分词便于内容检索
- 无缝集成下载工具

---

## 技术架构

### 技术栈
- **框架**: ASP.NET Core 9.0
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
  ├── Services (领域服务接口)
  └── Repositories (仓储接口)

DFApp.Application (应用层)
  ├── AppServices (应用服务)
  ├── Background Workers (后台任务)
  ├── DTOs (数据传输对象)
  └── Service Implementations (服务实现)

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

#### 1. AppRssSources (RSS源表)
```sql
CREATE TABLE "AppRssSources" (
    "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,                          -- RSS源名称
    "Url" TEXT NOT NULL,                           -- RSS Feed URL
    "ProxyUrl" TEXT,                               -- 代理地址
    "ProxyUsername" TEXT,                          -- 代理用户名
    "ProxyPassword" TEXT,                          -- 代理密码
    "IsEnabled" INTEGER NOT NULL DEFAULT 1,        -- 是否启用
    "FetchIntervalMinutes" INTEGER NOT NULL,        -- 抓取间隔（分钟）
    "MaxItems" INTEGER NOT NULL,                    -- 最大抓取条目数
    "Query" TEXT,                                  -- 搜索关键词（用于过滤）
    "LastFetchTime" TEXT,                          -- 最后抓取时间
    "FetchStatus" INTEGER NOT NULL DEFAULT 0,      -- 抓取状态（0=正常, 1=抓取中, 2=失败）
    "ErrorMessage" TEXT,                           -- 错误信息
    "Remark" TEXT,                                 -- 备注
    "ExtraProperties" TEXT NOT NULL DEFAULT '',    -- 额外属性（JSON）
    "ConcurrencyStamp" TEXT NOT NULL DEFAULT '',   -- 并发标记
    "CreationTime" TEXT NOT NULL,                  -- 创建时间
    "CreatorId" TEXT                               -- 创建者ID
);

CREATE INDEX "IX_AppRssSources_IsEnabled" ON "AppRssSources" ("IsEnabled");
CREATE INDEX "IX_AppRssSources_CreationTime" ON "AppRssSources" ("CreationTime" DESC);
```

#### 2. AppRssMirrorItems (RSS镜像条目表)
```sql
CREATE TABLE "AppRssMirrorItems" (
    "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "RssSourceId" INTEGER NOT NULL,                -- 关联的RSS源ID
    "Title" TEXT NOT NULL,                         -- 标题
    "Link" TEXT NOT NULL,                          -- 链接
    "Description" TEXT,                            -- 描述
    "Author" TEXT,                                 -- 作者
    "Category" TEXT,                               -- 分类
    "PublishDate" TEXT,                            -- 发布日期
    "Seeders" INTEGER,                             -- 做种人数（BT专用）
    "Leechers" INTEGER,                            -- 下载人数（BT专用）
    "Downloads" INTEGER,                           -- 完成下载次数（BT专用）
    "Extensions" TEXT,                             -- 扩展信息（JSON）
    "IsDownloaded" INTEGER NOT NULL DEFAULT 0,     -- 是否已下载
    "DownloadTime" TEXT,                           -- 下载时间
    "CreationTime" TEXT NOT NULL,                  -- 创建时间
    "LastModificationTime" TEXT,                   -- 最后修改时间
    "ConcurrencyStamp" TEXT NOT NULL DEFAULT '',   -- 并发标记
    FOREIGN KEY ("RssSourceId") REFERENCES "AppRssSources" ("Id")
);

CREATE INDEX "IX_AppRssMirrorItems_RssSourceId" ON "AppRssMirrorItems" ("RssSourceId");
CREATE INDEX "IX_AppRssMirrorItems_CreationTime" ON "AppRssMirrorItems" ("CreationTime" DESC);
CREATE INDEX "IX_AppRssMirrorItems_IsDownloaded" ON "AppRssMirrorItems" ("IsDownloaded");
CREATE INDEX "IX_AppRssMirrorItems_Title" ON "AppRssMirrorItems" ("Title");
```

#### 3. AppRssWordSegments (分词统计表)
```sql
CREATE TABLE "AppRssWordSegments" (
    "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "RssMirrorItemId" INTEGER NOT NULL,            -- 关联的镜像条目ID
    "Word" TEXT NOT NULL,                          -- 分词
    "LanguageType" INTEGER NOT NULL,               -- 语言类型（0=中文, 1=英文, 2=日文）
    "Count" INTEGER NOT NULL,                      -- 出现次数
    "PartOfSpeech" TEXT,                           -- 词性
    "CreationTime" TEXT NOT NULL,                  -- 创建时间
    "CreatorId" TEXT,                              -- 创建者ID
    FOREIGN KEY ("RssMirrorItemId") REFERENCES "AppRssMirrorItems" ("Id")
);

CREATE INDEX "IX_AppRssWordSegments_RssMirrorItemId" ON "AppRssWordSegments" ("RssMirrorItemId");
CREATE INDEX "IX_AppRssWordSegments_Word" ON "AppRssWordSegments" ("Word" COLLATE NOCASE);
CREATE INDEX "IX_AppRssWordSegments_LanguageType" ON "AppRssWordSegments" ("LanguageType");
```

### 关系说明
- 一个RSS源可以有多个镜像条目（1:N）
- 一个镜像条目可以有多个分词记录（1:N）
- 分词表使用不区分大小写的索引来支持忽略大小写的搜索

---

## 后端文件结构

### Domain层

#### `/src/DFApp.Domain/Rss/`

**RssSource.cs**
```csharp
/// <summary>
/// RSS源实体
/// </summary>
public class RssSource : Entity<long>, IHasCreationTime, IHasConcurrencyStamp
{
    public string Name { get; set; }                    // RSS源名称
    public string Url { get; set; }                     // RSS Feed URL
    public string? ProxyUrl { get; set; }               // 代理地址
    public string? ProxyUsername { get; set; }          // 代理用户名
    public string? ProxyPassword { get; set; }          // 代理密码
    public bool IsEnabled { get; set; }                 // 是否启用
    public int FetchIntervalMinutes { get; set; }       // 抓取间隔（分钟）
    public int MaxItems { get; set; }                   // 最大条目数
    public string? Query { get; set; }                  // 搜索关键词
    public DateTime? LastFetchTime { get; set; }        // 最后抓取时间
    public int FetchStatus { get; set; }                // 抓取状态
    public string? ErrorMessage { get; set; }           // 错误信息
    public string? Remark { get; set; }                 // 备注
    public string ExtraProperties { get; set; }         // 额外属性
    public string ConcurrencyStamp { get; set; }        // 并发标记
    public DateTime CreationTime { get; set; }          // 创建时间
    public Guid? CreatorId { get; set; }                // 创建者ID
}
```

**RssMirrorItem.cs**
```csharp
/// <summary>
/// RSS镜像条目实体
/// </summary>
public class RssMirrorItem : Entity<long>, IHasCreationTime, IHasModificationTime, IHasConcurrencyStamp
{
    public long RssSourceId { get; set; }               // 关联的RSS源ID
    public string Title { get; set; }                   // 标题
    public string Link { get; set; }                    // 链接
    public string? Description { get; set; }            // 描述
    public string? Author { get; set; }                 // 作者
    public string? Category { get; set; }               // 分类
    public DateTimeOffset? PublishDate { get; set; }    // 发布日期
    public int? Seeders { get; set; }                   // 做种人数
    public int? Leechers { get; set; }                  // 下载人数
    public int? Downloads { get; set; }                 // 完成下载次数
    public string? Extensions { get; set; }             // 扩展信息
    public bool IsDownloaded { get; set; }              // 是否已下载
    public DateTime? DownloadTime { get; set; }         // 下载时间
    public DateTime CreationTime { get; set; }          // 创建时间
    public DateTime? LastModificationTime { get; set; } // 最后修改时间
    public string ConcurrencyStamp { get; set; }        // 并发标记
}
```

**RssWordSegment.cs**
```csharp
/// <summary>
/// 分词统计实体
/// </summary>
public class RssWordSegment : Entity<long>, IHasCreationTime
{
    public long RssMirrorItemId { get; set; }           // 关联的镜像条目ID
    public string Word { get; set; }                    // 分词
    public int LanguageType { get; set; }               // 语言类型
    public int Count { get; set; }                      // 出现次数
    public string? PartOfSpeech { get; set; }           // 词性
    public DateTime CreationTime { get; set; }          // 创建时间
    public Guid? CreatorId { get; set; }                // 创建者ID
}
```

**IWordSegmentService.cs**
```csharp
/// <summary>
/// 分词服务接口
/// </summary>
public interface IWordSegmentService
{
    /// <summary>
    /// 对文本进行分词
    /// </summary>
    List<WordSegmentResult> Segment(string text);

    /// <summary>
    /// 对文本进行分词并统计词频
    /// </summary>
    Dictionary<string, int> SegmentAndCount(string text);
}

public class WordSegmentResult
{
    public string Word { get; set; } = string.Empty;
    public int LanguageType { get; set; }
    public string? PartOfSpeech { get; set; }
}
```

### Application层

#### `/src/DFApp.Application/Rss/`

**WordSegmentService.cs**
- 实现了多语言分词服务
- 支持中文（基于CJK字符）、英文（基于单词）、日文（基于假名）
- 自动检测语言类型
- 词频统计

**RssMirrorFetchWorker.cs**
```csharp
/// <summary>
/// RSS镜像抓取后台任务
/// 继承自QuartzBackgroundWorkerBase
/// 默认每5分钟执行一次
/// </summary>
public class RssMirrorFetchWorker : QuartzBackgroundWorkerBase
{
    protected override async Task Execute(IJobExecutionContext context)
    {
        // 1. 获取所有启用的RSS源
        // 2. 遍历每个RSS源进行抓取
        // 3. 解析RSS/Atom Feed
        // 4. 对每个条目进行分词
        // 5. 存储到数据库
    }
}
```

**RssSourceAppService.cs**
```csharp
/// <summary>
/// RSS源应用服务
/// </summary>
[Authorize(DFAppPermissions.Rss.Default)]
public class RssSourceAppService : ApplicationService, IRssSourceAppService
{
    // CRUD操作
    Task<PagedResultDto<RssSourceDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    Task<RssSourceDto> GetAsync(long id);
    Task<RssSourceDto> CreateAsync(CreateUpdateRssSourceDto input);
    Task<RssSourceDto> UpdateAsync(long id, CreateUpdateRssSourceDto input);
    Task DeleteAsync(long id);
    Task TriggerFetchAsync(long id);  // 手动触发抓取
}
```

**RssMirrorItemAppService.cs**
```csharp
/// <summary>
/// RSS镜像条目应用服务
/// </summary>
[Authorize(DFAppPermissions.Rss.Default)]
public class RssMirrorItemAppService : ApplicationService, IRssMirrorItemAppService
{
    // 查询操作
    Task<PagedResultDto<RssMirrorItemDto>> GetListAsync(GetRssMirrorItemsRequestDto input);
    Task<RssMirrorItemDto> GetAsync(long id);

    // 删除操作
    Task DeleteAsync(long id);
    Task DeleteManyAsync(List<long> ids);
    Task ClearAllAsync();

    // 统计操作
    Task<List<WordSegmentStatisticsDto>> GetWordSegmentStatisticsAsync(
        long? rssSourceId, int? languageType, int top);

    // 按分词查询
    Task<PagedResultDto<RssMirrorItemDto>> GetByWordTokenAsync(
        string wordToken, PagedAndSortedResultRequestDto input);

    // 下载到Aria2
    Task<string> DownloadToAria2Async(long id, bool videoOnly, bool enableKeywordFilter);
}
```

**RssWordSegmentAppService.cs**
```csharp
/// <summary>
/// RSS分词应用服务
/// </summary>
[Authorize(DFAppPermissions.Rss.Default)]
public class RssWordSegmentAppService : ApplicationService
{
    // 获取分词列表（分页）
    Task<PagedResultDto<RssWordSegmentWithItemDto>> GetListAsync(
        GetRssWordSegmentsRequestDto input);

    // 获取分词统计
    Task<List<WordSegmentStatisticsDto>> GetStatisticsAsync(
        long? rssSourceId, int? languageType, int top);

    // 删除操作
    Task DeleteByItemAsync(long rssMirrorItemId);
    Task DeleteBySourceAsync(long rssSourceId);
}
```

#### `/src/DFApp.Application/Background/`
- **RssMirrorFetchWorker.cs** - Quartz后台任务，定时抓取RSS源

### EntityFrameworkCore层

**DFAppDbContext.cs** 添加了：
```csharp
public DbSet<RssSource> RssSources { get; set; }
public DbSet<RssMirrorItem> RssMirrorItems { get; set; }
public DbSet<RssWordSegment> RssWordSegments { get; set; }
```

实体配置包括：
- 索引配置（提升查询性能）
- 外键关系配置
- 表名映射

### Application.Contracts层

#### `/src/DFApp.Application.Contracts/Rss/`

**DTO定义**：
- `RssSourceDto` - RSS源数据传输对象
- `CreateUpdateRssSourceDto` - 创建/更新RSS源DTO
- `RssMirrorItemDto` - 镜像条目DTO
- `RssWordSegmentDto` - 分词DTO
- `GetRssMirrorItemsRequestDto` - 查询请求DTO
- `WordSegmentStatisticsDto` - 分词统计DTO

**服务接口**：
- `IRssSourceAppService` - RSS源服务接口
- `IRssMirrorItemAppService` - 镜像条目服务接口

---

## API接口文档

### RSS源管理 API

#### 1. 获取RSS源列表
```http
GET /api/app/rss-source
```

**Query参数**:
- `skipCount`: 跳过条数（分页）
- `maxResultCount`: 最大返回条数（分页）
- `sorting`: 排序（例如：`"CreationTime desc"`）
- `filter`: 过滤关键词（搜索名称或URL）

**响应**:
```json
{
  "totalCount": 10,
  "items": [
    {
      "id": 1,
      "name": "Sukebei Nyaa",
      "url": "https://sukebei.nyaa.si/?page=rss",
      "isEnabled": true,
      "fetchIntervalMinutes": 5,
      "maxItems": 50,
      "lastFetchTime": "2026-01-14T10:30:00Z",
      "fetchStatus": 0,
      "creationTime": "2026-01-14T08:00:00Z"
    }
  ]
}
```

#### 2. 获取RSS源详情
```http
GET /api/app/rss-source/{id}
```

**响应**: 单个RSS源对象

#### 3. 创建RSS源
```http
POST /api/app/rss-source
```

**请求体**:
```json
{
  "name": "Sukebei Nyaa",
  "url": "https://sukebei.nyaa.si/?page=rss",
  "isEnabled": true,
  "fetchIntervalMinutes": 5,
  "maxItems": 50,
  "query": "optional keyword filter",
  "proxyUrl": "http://127.0.0.1:7890",
  "proxyUsername": "",
  "proxyPassword": "",
  "remark": "Optional remark"
}
```

**权限**: `DFAppPermissions.Rss.Create`

#### 4. 更新RSS源
```http
PUT /api/app/rss-source/{id}
```

**请求体**: 同创建RSS源

**权限**: `DFAppPermissions.Rss.Update`

#### 5. 删除RSS源
```http
DELETE /api/app/rss-source/{id}
```

**权限**: `DFAppPermissions.Rss.Delete`

#### 6. 手动触发抓取
```http
POST /api/app/rss-source/{id}/trigger-fetch
```

**说明**: 立即触发RSS源抓取（后台任务会在下次执行时处理）

---

### RSS镜像条目 API

#### 1. 获取镜像条目列表
```http
GET /api/app/rss-mirror-item
```

**Query参数**:
- `skipCount`: 跳过条数（分页）
- `maxResultCount`: 最大返回条数（分页）
- `sorting`: 排序
- `rssSourceId`: RSS源ID筛选
- `filter`: 关键词搜索（标题或描述）
- `wordToken`: 分词筛选
- `startTime`: 开始时间（ISO 8601）
- `endTime`: 结束时间（ISO 8601）
- `isDownloaded`: 下载状态筛选

**响应**:
```json
{
  "totalCount": 100,
  "items": [
    {
      "id": 1,
      "rssSourceId": 1,
      "rssSourceName": "Sukebei Nyaa",
      "title": "[Example] Title Here",
      "link": "https://example.com/download/123",
      "description": "Description here",
      "author": "Author Name",
      "category": "Category",
      "publishDate": "2026-01-14T08:00:00Z",
      "seeders": 100,
      "leechers": 50,
      "downloads": 200,
      "isDownloaded": false,
      "creationTime": "2026-01-14T10:00:00Z",
      "wordSegments": [
        {
          "word": "example",
          "languageType": 1,
          "count": 2,
          "creationTime": "2026-01-14T10:00:00Z"
        }
      ]
    }
  ]
}
```

#### 2. 获取条目详情
```http
GET /api/app/rss-mirror-item/{id}
```

#### 3. 删除单个条目
```http
DELETE /api/app/rss-mirror-item/{id}
```

**权限**: `DFAppPermissions.Rss.Delete`

#### 4. 批量删除条目
```http
DELETE /api/app/rss-mirror-item/many
```

**请求体**: `[1, 2, 3]` (ID数组)

**权限**: `DFAppPermissions.Rss.Delete`

#### 5. 清空所有条目
```http
POST /api/app/rss-mirror-item/clear-all
```

**说明**: 删除所有镜像条目和关联的分词记录

#### 6. 获取分词统计
```http
GET /api/app/rss-mirror-item/word-segment-statistics
```

**Query参数**:
- `rssSourceId`: RSS源ID筛选（可选）
- `languageType`: 语言类型（0=中文, 1=英文, 2=日文，可选）
- `top`: 返回Top N（默认100）

**响应**:
```json
[
  {
    "word": "example",
    "totalCount": 150,
    "itemCount": 80,
    "languageType": 1
  }
]
```

#### 7. 按分词查询条目
```http
GET /api/app/rss-mirror-item/by-word-token
```

**Query参数**:
- `wordToken`: 分词
- `skipCount`: 跳过条数
- `maxResultCount`: 最大返回条数
- `sorting`: 排序

#### 8. 下载到Aria2
```http
POST /api/app/rss-mirror-item/{id}/download-to-aria2
```

**Query参数**:
- `videoOnly`: 是否仅下载视频（默认false）
- `enableKeywordFilter`: 是否启用关键词过滤（默认false）

**响应**: Aria2任务GID

**说明**: 会将条目标记为已下载

---

### RSS分词统计 API

#### 1. 获取分词列表（分页）
```http
GET /api/app/rss-word-segment
```

**Query参数**:
- `skipCount`: 跳过条数（分页）
- `maxResultCount`: 最大返回条数（分页）
- `sorting`: 排序（例如：`"CreationTime desc"`）
- `filter`: 过滤关键词（搜索分词）
- `rssSourceId`: RSS源ID筛选（可选）
- `languageType`: 语言类型（0=中文, 1=英文, 2=日文，可选）
- `word`: 分词文本精确匹配（可选）

**响应**:
```json
{
  "totalCount": 1000,
  "items": [
    {
      "id": 1,
      "rssMirrorItemId": 10,
      "rssMirrorItemTitle": "[Example] Title Here",
      "rssMirrorItemLink": "https://example.com/download/123",
      "rssSourceId": 1,
      "rssSourceName": "Sukebei Nyaa",
      "word": "example",
      "languageType": 1,
      "count": 2,
      "partOfSpeech": "noun",
      "creationTime": "2026-01-14T10:00:00Z"
    }
  ]
}
```

#### 2. 获取分词统计（分页）
```http
GET /api/app/rss-word-segment/statistics
```

**Query参数**:
- `skipCount`: 跳过条数（分页）
- `maxResultCount`: 最大返回条数（分页）
- `sorting`: 排序（例如：`"totalCount desc"`）
- `filter`: 过滤关键词（搜索分词，模糊匹配）
- `rssSourceId`: RSS源ID筛选（可选）
- `languageType`: 语言类型（0=中文, 1=英文, 2=日文，可选）
- `word`: 分词文本精确匹配（可选，用于点击查看按钮时）

**响应**:
```json
{
  "totalCount": 5000,
  "items": [
    {
      "word": "example",
      "totalCount": 150,
      "itemCount": 80,
      "languageType": 1
    }
  ]
}
```

**说明**:
- 返回全部分词统计，支持分页
- `totalCount`: 总的不同分词数量
- `word`: 分词文本（去重后）
- `totalCount`: 该词在所有条目中出现的总次数
- `itemCount`: 包含该词的不同条目数量
- 按分词的小写形式进行去重（GroupBy）

#### 3. 删除指定条目的所有分词
```http
DELETE /api/app/rss-word-segment/by-item/{rssMirrorItemId}
```

**权限**: `DFAppPermissions.Rss.Delete`

#### 4. 删除指定RSS源的所有分词
```http
DELETE /api/app/rss-word-segment/by-source/{rssSourceId}
```

**权限**: `DFAppPermissions.Rss.Delete`

---

## 核心服务说明

### 1. 分词服务 (WordSegmentService)

#### 语言检测
- 统计CJK字符（中日韩统一表意文字）
- 统计英文字母
- 统计日文假名（平假名、片假名）
- 根据比例判断主导语言

#### 分词策略

**中文分词** (LanguageType = 0)
```csharp
private List<WordSegmentResult> SegmentChinese(string text)
{
    var results = new List<WordSegmentResult>();
    // 1. 提取连续的CJK字符
    // 2. 按字符分割（简单的单字符切分）
    // 3. 过滤停用词
    // 4. 统计词频
    return results;
}
```

**英文分词** (LanguageType = 1)
```csharp
private List<WordSegmentResult> SegmentEnglish(string text)
{
    var results = new List<WordSegmentResult>();
    // 1. 提取单词
    // 2. 转换为小写
    // 3. 过滤停用词（the, is, at, which, on等）
    // 4. 统计词频
    return results;
}
```

**日文分词** (LanguageType = 2)
```csharp
private List<WordSegmentResult> SegmentJapanese(string text)
{
    var results = new List<WordSegmentResult>();
    // 1. 提取假名字符
    // 2. 提取CJK字符
    // 3. 组合切分
    // 4. 统计词频
    return results;
}
```

#### 词频统计
```csharp
public Dictionary<string, int> SegmentAndCount(string text)
{
    var segments = Segment(text);
    var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    foreach (var segment in segments)
    {
        var key = segment.Word.ToLower();
        if (counts.ContainsKey(key))
            counts[key]++;
        else
            counts[key] = 1;
    }

    return counts;
}
```

### 2. RSS抓取后台任务 (RssMirrorFetchWorker)

#### Quartz配置
```csharp
public RssMirrorFetchWorker(...)
{
    JobDetail = JobBuilder.Create<RssMirrorFetchWorker>()
        .WithIdentity(nameof(RssMirrorFetchWorker))
        .Build();

    Trigger = TriggerBuilder.Create()
        .WithIdentity(nameof(RssMirrorFetchWorker))
        .WithSimpleSchedule(x => x
            .WithIntervalInMinutes(5)  // 每5分钟执行
            .RepeatForever())
        .Build();
}
```

#### 执行流程
```csharp
public override async Task Execute(IJobExecutionContext context)
{
    // 1. 获取所有启用的RSS源
    var enabledSources = await _rssSourceRepository.GetListAsync(s => s.IsEnabled);

    foreach (var source in enabledSources)
    {
        await FetchRssSource(source);
    }
}

private async Task FetchRssSource(RssSource source)
{
    using (var unitOfWork = _unitOfWorkManager.Begin())
    {
        try
        {
            // 1. 创建HttpClient并配置代理
            var httpClient = CreateHttpClient(source);

            // 2. 下载RSS Feed
            var feedXml = await httpClient.GetStringAsync(source.Url);

            // 3. 解析RSS XML
            var items = ParseRssXml(feedXml, source);

            // 4. 四步处理流程（避免外键约束问题）
            // 第一步：检查并准备新条目
            var newItems = new List<RssMirrorItem>();
            foreach (var item in items)
            {
                var existing = await _rssMirrorItemRepository.FirstOrDefaultAsync(i => i.Link == item.Link);
                if (existing == null)
                {
                    item.RssSourceId = source.Id;
                    item.CreationTime = DateTime.Now;
                    newItems.Add(item);
                }
            }

            // 第二步：批量插入镜像条目
            foreach (var item in newItems)
            {
                await _rssMirrorItemRepository.InsertAsync(item);
            }

            // 第三步：保存更改以生成ID
            await unitOfWork.SaveChangesAsync();

            // 第四步：插入分词数据
            foreach (var item in newItems)
            {
                var wordSegments = _wordSegmentService.Segment(item.Title);
                var segmentDict = _wordSegmentService.SegmentAndCount(item.Title);

                foreach (var segment in wordSegments)
                {
                    var rssWordSegment = new RssWordSegment
                    {
                        RssMirrorItemId = item.Id,  // ID已生成
                        Word = segment.Word,
                        LanguageType = segment.LanguageType,
                        Count = segmentDict.TryGetValue(segment.Word.ToLower(), out var count) ? count : 1,
                        CreationTime = DateTime.Now
                    };
                    await _rssWordSegmentRepository.InsertAsync(rssWordSegment);
                }
            }

            // 5. 更新RSS源状态（重新获取以避免并发问题）
            var currentSource = await _rssSourceRepository.GetAsync(source.Id);
            currentSource.LastFetchTime = DateTime.Now;
            currentSource.FetchStatus = 0;  // 成功
            currentSource.ErrorMessage = null;
            await _rssSourceRepository.UpdateAsync(currentSource);

            await unitOfWork.CompleteAsync();

            Logger.LogInformation("RSS源 {Name} 抓取完成，新增 {Count} 条记录", source.Name, newItems.Count);
        }
        catch (Exception ex)
        {
            // 更新失败状态
            var currentSource = await _rssSourceRepository.GetAsync(source.Id);
            currentSource.FetchStatus = 2;  // 失败
            currentSource.ErrorMessage = ex.Message;
            await _rssSourceRepository.UpdateAsync(currentSource);

            Logger.LogError(ex, "抓取RSS源 {Name} 失败", source.Name);
        }
    }
}
```

#### 代理支持
```csharp
private HttpClient CreateHttpClient(RssSource source)
{
    var client = new HttpClient();

    if (!string.IsNullOrEmpty(source.ProxyUrl))
    {
        var proxy = new WebProxy
        {
            Address = new Uri(source.ProxyUrl)
        };

        if (!string.IsNullOrEmpty(source.ProxyUsername))
        {
            proxy.Credentials = new NetworkCredential(
                source.ProxyUsername,
                source.ProxyPassword
            );
        }

        // 注意：实际使用时需要通过HttpClientHandler设置代理
    }

    return client;
}
```

### 3. Aria2集成

**RssMirrorItemAppService.DownloadToAria2Async**:
```csharp
public async Task<string> DownloadToAria2Async(
    long id,
    bool videoOnly = false,
    bool enableKeywordFilter = false)
{
    // 1. 获取镜像条目
    var item = await _rssMirrorItemRepository.GetAsync(id);

    if (item.IsDownloaded)
    {
        throw new UserFriendlyException("该条目已经下载过");
    }

    // 2. 创建Aria2下载请求
    var request = new AddDownloadRequestDto
    {
        Urls = new List<string> { item.Link },
        VideoOnly = videoOnly,
        EnableKeywordFilter = enableKeywordFilter
    };

    // 3. 调用Aria2服务
    var result = await _aria2Service.AddDownloadAsync(request);

    // 4. 更新下载状态
    item.IsDownloaded = true;
    item.DownloadTime = DateTime.Now;
    await _rssMirrorItemRepository.UpdateAsync(item);

    Logger.LogInformation("RSS镜像条目 {Id} 已添加到Aria2下载队列", id);

    return result.Id;
}
```

---

## 权限定义

**文件**: `/src/DFApp.Application.Contracts/Permissions/DFAppPermissions.cs`

```csharp
public static class Rss
{
    public const string Default = GroupName + ".Rss";
    public const string Create = Default + ".Create";
    public const string Update = Default + ".Update";
    public const string Delete = Default + ".Delete";
    public const string Download = Default + ".Download";
}
```

**权限说明**:
- `DFApp.Rss` - 基础权限，查看RSS源和镜像条目
- `DFApp.Rss.Create` - 创建RSS源
- `DFApp.Rss.Update` - 更新RSS源
- `DFApp.Rss.Delete` - 删除RSS源和镜像条目
- `DFApp.Rss.Download` - 下载到Aria2

---

## 配置说明

### Quartz调度器配置

**appsettings.json**:
```json
{
  "Quartz": {
    "Enabled": true
  }
}
```

### 数据库连接

**appsettings.json**:
```json
{
  "ConnectionStrings": {
    "Default": "Data Source=DFApp.db"
  }
}
```

### 日志配置

**appsettings.json**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "DFApp.Rss": "Debug"
    }
  }
}
```

---

## 使用流程

### 1. 配置RSS源

**步骤**:
1. 登录系统
2. 导航到"下载与订阅" → "RSS源管理"
3. 点击"新增RSS源"
4. 填写配置信息：
   - **名称**: 例如 "Sukebei Nyaa"
   - **URL**: RSS Feed地址，例如 "https://sukebei.nyaa.si/?page=rss"
   - **是否启用**: 勾选
   - **抓取间隔**: 例如 5 分钟
   - **最大条目数**: 例如 50
   - **搜索关键词**: 可选，用于过滤Feed中的条目
   - **代理配置**: 如果需要代理访问，填写代理信息
5. 点击"确定"保存

**说明**: 保存后，后台任务会在下一个调度周期自动抓取

### 2. 手动触发抓取

**步骤**:
1. 在RSS源列表中找到目标RSS源
2. 点击操作列的"立即抓取"按钮
3. 后台任务会在下次执行时优先处理该RSS源

### 3. 查看镜像条目

**步骤**:
1. 导航到"下载与订阅" → "RSS镜像条目"
2. 使用筛选条件查找：
   - 选择RSS源
   - 输入关键词搜索标题
   - 选择下载状态
   - 设置时间范围
   - 输入分词快速过滤
3. 点击"搜索"按钮

### 4. 下载内容

**步骤**:
1. 在镜像条目列表中找到目标条目
2. 点击操作列的"下载"按钮
3. 在弹出的对话框中配置下载选项：
   - **仅下载视频**: 勾选后，下载.torrent文件时只选择视频文件
   - **启用关键词过滤**: 勾选后，根据关键词过滤规则过滤文件
4. 点击"确定"添加到Aria2下载队列

### 5. 使用分词统计页面

**页面入口**: "下载与订阅" → "分词统计"

**功能说明**:
这是一个专门的分词统计和查询页面，采用左右分栏布局：

**左侧 - 全部分词统计**:
- 显示所有分词的统计信息（已去重）
- 按总出现次数降序排列
- 支持分页浏览（20/50/100/200条/页）
- 显示每个分词的：
  - 排名
  - 分词文本
  - 语言类型（中文/英文/日文）
  - 总出现次数
  - 所在条目数
- 点击"查看"按钮：在右侧精确显示该分词的所有条目

**右侧 - 分词详情列表**:
- 显示具体的分词记录
- 支持按分词精确查看
- 显示每个分词的：
  - 分词文本
  - 语言类型
  - RSS源名称
  - 条目标题
  - 出现次数
  - 创建时间
- 操作按钮：
  - **打开**：在新窗口打开条目链接
  - **下载**：直接下载到Aria2
  - **查看**：按该分词精确筛选

**筛选功能**:

1. **关键词搜索**（模糊匹配）:
   - 在搜索框输入关键词
   - 左侧显示包含该关键词的所有分词统计
   - 右侧显示包含该关键词的所有分词记录
   - 提示栏显示：🔍 正在搜索包含"xxx"的分词（左侧统计 X 个，右侧详情 X 条）

2. **语言筛选**:
   - 选择语言类型（中文/英文/日文）
   - 只显示该语言的分词
   - 提示栏显示：🌐 筛选语言：中文（左侧统计 X 个，右侧详情 X 条）

3. **精确查看**:
   - 点击左侧统计表的"查看"按钮
   - 右侧切换为精确匹配该分词
   - 提示栏显示：📌 正在查看分词："xxx"（精确匹配，共 X 条记录）

4. **重置筛选**:
   - 点击"重置"按钮
   - 清空所有筛选条件
   - 显示全部分词

**使用场景**:

1. **内容分析**: 通过分词统计了解RSS源的主题分布
2. **快速定位**: 搜索特定分词，快速找到相关内容
3. **批量下载**: 在分词详情页面直接下载感兴趣的条目

### 6. 管理条目

**批量删除**:
1. 勾选要删除的条目
2. 点击"批量删除"按钮
3. 确认删除

**清空所有**:
1. 点击"清空所有"按钮
2. 确认删除（此操作不可恢复）

---

## 技术细节

### 1. 对象映射 (Riok.Mapperly)

**文件**: `/src/DFApp.Application/DFAppApplicationMappers.cs`

```csharp
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class RssSourceToRssSourceDtoMapper : MapperBase<RssSource, RssSourceDto>
{
    public override partial RssSourceDto Map(RssSource source);
    public override partial void Map(RssSource source, RssSourceDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class CreateUpdateRssSourceDtoToRssSourceMapper : MapperBase<CreateUpdateRssSourceDto, RssSource>
{
    [MapperIgnoreTarget(nameof(RssSource.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(RssSource.CreationTime))]
    [MapperIgnoreTarget(nameof(RssSource.CreatorId))]
    public override partial RssSource Map(CreateUpdateRssSourceDto source);
    public override partial void Map(CreateUpdateRssSourceDto source, RssSource destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class RssMirrorItemToRssMirrorItemDtoMapper : MapperBase<RssMirrorItem, RssMirrorItemDto>
{
    public override partial RssMirrorItemDto Map(RssMirrorItem source);
    public override partial void Map(RssMirrorItem source, RssMirrorItemDto destination);
}
```

**说明**:
- 使用Riok.Mapperly进行编译时映射
- 自动生成映射代码，性能优于反射
- IgnoreTarget用于忽略目标字段在创建时不赋值的情况

### 2. 异步查询模式

**ABP Framework AsyncExecuter**:
```csharp
public async Task<PagedResultDto<RssSourceDto>> GetListAsync(PagedAndSortedResultRequestDto input)
{
    var queryable = await _rssSourceRepository.GetQueryableAsync();

    // 动态排序
    if (!string.IsNullOrWhiteSpace(input.Sorting))
    {
        queryable = queryable.OrderBy(input.Sorting);  // 需要System.Linq.Dynamic.Core
    }
    else
    {
        queryable = queryable.OrderByDescending(x => x.CreationTime);
    }

    // 异步分页查询
    var totalCount = await AsyncExecuter.CountAsync(queryable);
    var items = await AsyncExecuter.ToListAsync(
        queryable.Skip(input.SkipCount).Take(input.MaxResultCount)
    );

    var dtos = ObjectMapper.Map<List<RssSource>, List<RssSourceDto>>(items);

    return new PagedResultDto<RssSourceDto>(totalCount, dtos);
}
```

**关键点**:
- 必须使用 `AsyncExecuter.CountAsync()` 和 `AsyncExecuter.ToListAsync()`
- 不能使用 `Count()` 和 `ToList()`
- 动态排序需要引用 `System.Linq.Dynamic.Core`

### 3. 并发控制

**ConcurrencyStamp**:
```csharp
public class RssSource : Entity<long>, IHasCreationTime, IHasConcurrencyStamp
{
    public string ConcurrencyStamp { get; set; } = string.Empty;
}
```

**更新时**:
```csharp
public async Task<RssSourceDto> UpdateAsync(long id, CreateUpdateRssSourceDto input)
{
    var source = await _rssSourceRepository.GetAsync(id);

    // AutoMapper映射
    ObjectMapper.Map(input, source);

    // 更新并发标记
    source.ConcurrencyStamp = Guid.NewGuid().ToString();

    await _rssSourceRepository.UpdateAsync(source);

    return ObjectMapper.Map<RssSource, RssSourceDto>(source);
}
```

**并发异常处理**:
在后台任务中，直接更新从UnitOfWork开始前查询的实体会导致并发异常：
```csharp
// ❌ 错误做法
source.FetchStatus = 0;
await _rssSourceRepository.UpdateAsync(source);  // 可能抛出并发异常

// ✅ 正确做法
var currentSource = await _rssSourceRepository.GetAsync(source.Id);
currentSource.FetchStatus = 0;
await _rssSourceRepository.UpdateAsync(currentSource);  // 使用最新实体
```

### 4. UnitOfWork和ID生成

**问题**: 在ABP的UnitOfWork中，`InsertAsync` 不会立即执行SQL INSERT，而是在 `CompleteAsync()` 时批量保存。这导致插入后实体的ID还未生成。

**解决方案**: 使用四步处理流程

```csharp
using (var unitOfWork = _unitOfWorkManager.Begin())
{
    var newItems = new List<RssMirrorItem>();

    // 第一步：准备数据
    foreach (var item in items)
    {
        var existing = await _repository.FirstOrDefaultAsync(i => i.Link == item.Link);
        if (existing == null)
        {
            item.RssSourceId = source.Id;
            item.CreationTime = DateTime.Now;
            newItems.Add(item);
        }
    }

    // 第二步：批量插入
    foreach (var item in newItems)
    {
        await _repository.InsertAsync(item);
    }

    // 第三步：保存更改以生成ID ⚠️ 关键步骤
    await unitOfWork.SaveChangesAsync();

    // 第四步：使用已生成的ID插入关联数据
    foreach (var item in newItems)
    {
        var segment = new RssWordSegment
        {
            RssMirrorItemId = item.Id,  // ✅ ID已生成
            Word = word,
            Count = 1
        };
        await _wordSegmentRepository.InsertAsync(segment);
    }

    await unitOfWork.CompleteAsync();
}
```

**关键点**:
- `SaveChangesAsync()` 会触发数据库INSERT语句，生成自增ID
- 调用后，实体的 `Id` 属性会被填充
- 之后才能使用该ID插入关联数据
- `CompleteAsync()` 提交UnitOfWork事务


### 5. 关键词过滤

**Query字段**:
```csharp
public class RssSource
{
    public string? Query { get; set; }  // 搜索关键词
}
```

**使用示例**:
- 如果Query设置为 `"-raw -censored"`
- 抓取时会过滤标题中包含"raw"或"censored"的条目

### 6. 分词去重

**分词表索引**:
```sql
CREATE INDEX "IX_AppRssWordSegments_Word"
ON "AppRssWordSegments" ("Word" COLLATE NOCASE);
```

**查询时**:
```csharp
var wordSegmentQueryable = await _rssWordSegmentRepository.GetQueryableAsync();
var filterItemIds = wordSegmentQueryable
    .Where(x => x.Word.ToLower() == input.WordToken.ToLower())
    .Select(x => x.RssMirrorItemId)
    .Distinct();

queryable = queryable.Where(x => filterItemIds.Contains(x.Id));
```

**说明**:
- 使用 `COLLATE NOCASE` 创建不区分大小写的索引
- 查询时使用 `ToLower()` 确保忽略大小写
- `Distinct()` 确保不重复返回条目

### 6. 扩展信息存储

**Extensions字段**:
```csharp
public class RssMirrorItem
{
    public string? Extensions { get; set; }  // JSON格式的扩展信息
}
```

**可能的用途**:
- 存储BT种子的元数据（文件列表、大小等）
- 存储RSS Feed的自定义字段
- 存储解析后的额外信息

**示例**:
```json
{
  "files": [
    {"name": "video.mp4", "size": 1073741824},
    {"name": "video.nfo", "size": 1024}
  ],
  "totalSize": 1073742848,
  "infoHash": "abc123..."
}
```

---

## 故障排查

### 1. RSS抓取失败

**症状**: RSS源状态显示"失败"，错误信息显示超时或连接错误

**可能原因**:
- 网络连接问题
- RSS源URL无效
- 需要代理访问但未配置代理
- RSS源服务器暂时不可用

**解决方案**:
1. 检查RSS源URL是否正确
2. 如果RSS源在国外，尝试配置代理
3. 检查日志文件获取详细错误信息
4. 手动访问RSS URL验证其可访问性

**日志位置**:
```
Logs/DFApp.log
```

**相关日志**:
```
[ERROR] 抓取RSS源 Sukebei Nyaa 失败: System.Net.Http.HttpRequestException: Connection timeout
```

### 2. 分词统计不准确

**症状**: 分词统计显示的分词不符合预期

**可能原因**:
- 分词服务实现较简单，不支持复杂分词
- 停用词列表不完整
- 语言检测不准确

**解决方案**:
1. 当前实现是基于规则的简单分词，适合基本场景
2. 如需高级分词，可集成第三方分词库：
   - 中文：Jieba.Net、Lucene.Net.Analysis.SmartCn
   - 日文：MeCab.Net
   - 英文：NLTK、Stanford NLP

### 3. 后台任务未执行

**症状**: RSS源状态一直显示"正常"但LastFetchTime没有更新

**可能原因**:
- Quartz调度器未启用
- 后台任务注册失败
- 数据库中没有启用的RSS源

**解决方案**:
1. 检查 `appsettings.json` 中Quartz是否启用
2. 查看启动日志，确认后台任务是否成功注册
3. 确认至少有一个RSS源的 `IsEnabled = true`

**检查日志**:
```
[INFO] QuartzBackgroundWorkerManager registered background worker: RssMirrorFetchWorker
```

### 4. 下载到Aria2失败

**症状**: 点击下载按钮后提示"下载失败"

**可能原因**:
- Aria2服务未运行
- Aria2 RPC连接配置错误
- 链接无效或种子文件损坏

**解决方案**:
1. 检查Aria2服务状态
2. 验证Aria2 RPC配置
3. 尝试手动用浏览器打开链接验证其有效性
4. 检查Aria2日志

### 5. 分词搜索无结果

**症状**: 输入分词后搜索，但没有返回任何条目

**可能原因**:
- 分词输入错误（大小写、空格等）
- 该分词确实不存在于数据库中
- 分词表未正确生成

**解决方案**:
1. 在"分词统计"中查看是否有该分词
2. 尝试使用分词的一部分搜索
3. 检查镜像条目的分词详情（点击"查看分词"）
4. 如果分词表为空，重新触发RSS抓取

### 6. 权限未定义错误

**症状**: 启动后出现权限错误，类似：
```
System.InvalidOperationException: The AuthorizationPolicy named: 'DFApp.Rss.Create' was not found.
```

**可能原因**:
- 权限常量已定义但未在权限定义提供者中注册
- 本地化资源文件中缺少权限翻译

**解决方案**:
1. 确认 `DFAppPermissions.cs` 中定义了权限常量
2. 在 `DFAppPermissionDefinitionProvider.cs` 中注册权限子权限：
   ```csharp
   var rssPermission = rssGroup.AddPermission(DFAppPermissions.Rss.Default, L("Permission:Rss"));
   rssPermission.AddChild(DFAppPermissions.Rss.Create, L("Permission:Rss.Create"));
   rssPermission.AddChild(DFAppPermissions.Rss.Update, L("Permission:Rss.Update"));
   rssPermission.AddChild(DFAppPermissions.Rss.Delete, L("Permission:Rss.Delete"));
   rssPermission.AddChild(DFAppPermissions.Rss.Download, L("Permission:Rss.Download"));
   ```
3. 在本地化文件中添加翻译：
   - `src/DFApp.Domain.Shared/Localization/DFApp/zh-Hans.json`
   - `src/DFApp.Domain.Shared/Localization/DFApp/en.json`

**相关文件**:
- `/src/DFApp.Application.Contracts/Permissions/DFAppPermissionDefinitionProvider.cs`
- `/src/DFApp.Application.Contracts/Permissions/DFAppPermissions.cs`
- `/src/DFApp.Domain.Shared/Localization/DFApp/zh-Hans.json`
- `/src/DFApp.Domain.Shared/Localization/DFApp/en.json`

### 7. 外键约束失败和并发异常

**症状**:
1. 插入分词数据时出现外键约束错误：
```
SQLite Error 19: 'FOREIGN KEY constraint failed'
INSERT INTO "AppRssWordSegment" ...
```

2. 更新RSS源状态时出现并发异常：
```
Volo.Abp.Data.AbpDbConcurrencyException: The database operation was expected to affect 1 row(s), but actually affected 0 row(s)
```

**可能原因**:
- 在UnitOfWork中，`InsertAsync` 不会立即生成ID
- 直接更新从UnitOfWork开始前查询的实体导致并发标记冲突

**解决方案**:
已修复 `RssMirrorFetchWorker.cs`，采用四步处理流程：

1. **检查并准备新条目** - 过滤重复条目
2. **批量插入镜像条目** - 调用 `InsertAsync`
3. **保存更改生成ID** - 调用 `SaveChangesAsync()` 确保ID生成
4. **插入分词数据** - 使用已生成的ID

对于并发问题，在更新前重新从数据库获取最新实体：
```csharp
var currentSource = await _rssSourceRepository.GetAsync(source.Id);
currentSource.LastFetchTime = DateTime.Now;
await _rssSourceRepository.UpdateAsync(currentSource);
```

**相关代码**:
- `/src/DFApp.Application/Background/RssMirrorFetchWorker.cs:148-197`

---

## 性能优化建议

### 1. 数据库索引

已创建的索引：
```sql
-- RSS源表
CREATE INDEX "IX_AppRssSources_IsEnabled" ON "AppRssSources" ("IsEnabled");
CREATE INDEX "IX_AppRssSources_CreationTime" ON "AppRssSources" ("CreationTime" DESC);

-- 镜像条目表
CREATE INDEX "IX_AppRssMirrorItems_RssSourceId" ON "AppRssMirrorItems" ("RssSourceId");
CREATE INDEX "IX_AppRssMirrorItems_CreationTime" ON "AppRssMirrorItems" ("CreationTime" DESC);
CREATE INDEX "IX_AppRssMirrorItems_IsDownloaded" ON "AppRssMirrorItems" ("IsDownloaded");
CREATE INDEX "IX_AppRssMirrorItems_Title" ON "AppRssMirrorItems" ("Title");

-- 分词表
CREATE INDEX "IX_AppRssWordSegments_RssMirrorItemId" ON "AppRssWordSegments" ("RssMirrorItemId");
CREATE INDEX "IX_AppRssWordSegments_Word" ON "AppRssWordSegments" ("Word" COLLATE NOCASE);
CREATE INDEX "IX_AppRssWordSegments_LanguageType" ON "AppRssWordSegments" ("LanguageType");
```

### 2. 查询优化

**分页查询**:
```csharp
// 好的做法：先查询总数，再查询当前页
var totalCount = await AsyncExecuter.CountAsync(queryable);
var items = await AsyncExecuter.ToListAsync(
    queryable.Skip(input.SkipCount).Take(input.MaxResultCount)
);

// 避免：一次性查询所有数据再分页
var allItems = await AsyncExecuter.ToListAsync(queryable);  // 不要这样做！
```

**关联查询**:
```csharp
// 好的做法：分批加载关联数据
var itemIds = items.Select(i => i.Id).ToList();
var wordSegments = await _rssWordSegmentRepository.GetListAsync(
    x => itemIds.Contains(x.RssMirrorItemId)
);

// 避免：循环查询（N+1问题）
foreach (var item in items)
{
    var segments = await _rssWordSegmentRepository.GetListAsync(
        x => x.RssMirrorItemId == item.Id  // 不要这样做！
    );
}
```

### 3. 缓存策略

**RSS源列表缓存**:
```csharp
// 可考虑添加缓存
[Cache]  // ABP缓存特性
public async Task<List<RssSourceDto>> GetAllSourcesAsync()
{
    // ...
}
```

### 4. 批量操作

**批量插入**:
```csharp
// 如果一次插入大量分词数据，考虑使用批量插入
await _rssWordWordSegmentRepository.InsertManyAsync(wordSegments);
```

---

## 扩展建议

### 1. 支持更多Feed格式

当前支持：RSS 2.0, Atom 1.0

可扩展支持：
- RSS 1.0
- Media RSS
- Podcast feeds
- JSON Feed

### 2. 高级分词

集成专业分词库：
- **中文**: Jieba.Net、Lucene.Net.Analysis.SmartCn
- **日文**: MeCab.Net、NMeCab
- **英文**: NLTK、Stanford NLP（通过Python interop）

### 3. 智能推荐

基于分词统计和用户下载历史，推荐相关内容：
```csharp
public async Task<List<RssMirrorItemDto>> GetRecommendationsAsync(long userId)
{
    // 1. 获取用户下载历史
    // 2. 分析用户偏好（分词）
    // 3. 查找相似的新条目
    // 4. 返回推荐列表
}
```

### 4. 自动下载规则

配置规则自动下载：
```csharp
public class AutoDownloadRule
{
    public string Name { get; set; }
    public string KeywordFilter { get; set; }
    public int MinSeeders { get; set; }
    public bool AutoDownload { get; set; }
}
```

### 5. 通知推送

当RSS源有新内容时推送通知：
- SignalR实时推送
- 邮件通知
- Telegram Bot通知
- 企业微信/钉钉通知

### 6. 数据导出

支持导出功能：
- 导出为CSV/Excel
- 导出为JSON
- 导出为RSS Feed

### 7. 数据清理

定期清理旧数据：
```csharp
public async Task CleanupOldItemsAsync(TimeSpan retentionPeriod)
{
    var cutoffDate = DateTime.Now.Subtract(retentionPeriod);
    var oldItems = await _rssMirrorItemRepository.GetListAsync(
        x => x.CreationTime < cutoffDate && !x.IsDownloaded
    );

    await _rssMirrorItemRepository.DeleteManyAsync(oldItems);
}
```

---

## 版本历史

### v1.1.0 (2026-01-15)
**分词统计页面更新**

#### 新增功能
- 专门的分词统计页面（/download-subscription/rss-word-segments）
- 左右分栏布局设计
- 全部分词统计查询（支持分页，不限制数量）
- 分词详情列表查看
- 筛选条件实时提示栏
- 直接从分词详情页面下载到Aria2

#### 改进
- 分词统计API改为分页模式，支持获取全部分词
- 优化前端布局，动态计算表格高度
- 添加筛选状态提示，提升用户体验
- 支持模糊搜索和精确筛选两种模式

#### 技术细节
- RssWordSegmentAppService.GetStatisticsAsync 改为返回 PagedResultDto
- 前端使用 el-row/el-col 实现左右布局
- 使用 window resize 监听实现动态高度计算

---

### v1.0.0 (2026-01-14)
**初始版本**

#### 新增功能
- RSS源管理（CRUD）
- RSS镜像条目查询和管理
- 多语言分词服务（中文、英文、日文）
- Quartz后台定时抓取（默认5分钟）
- 分词统计和按分词过滤
- Aria2下载集成
- 代理支持（HTTP/SOCKS5）
- 关键词过滤（Query字段）

#### 技术栈
- ASP.NET Core 9.0
- ABP Framework
- Entity Framework Core
- Quartz.NET
- SQLite
- Riok.Mapperly

#### API接口
- 6个RSS源管理API
- 8个RSS镜像条目API

#### 权限定义
- 5个权限节点

---

## 相关资源

### 内部文档
- [ABP Framework官方文档](https://docs.abp.io/)
- [Quartz.NET文档](https://www.quartz-scheduler.net/documentation/)
- [Entity Framework Core文档](https://docs.microsoft.com/ef/core/)

### 外部资源
- [RSS 2.0规范](https://www.rssboard.org/rss-specification)
- [Atom 1.0规范](https://www.rfc-editor.org/rfc/rfc4287)
- [Aria2 RPC文档](https://aria2.github.io/manual/en/html/aria2c.html#rpc-interface)

---

## 作者信息

**开发日期**: 2026-01-14
**版本**: 1.0.0
**框架**: ASP.NET Core 9.0 + ABP Framework
**AI助手**: Claude (Anthropic)

---

## 许可证

本功能是DFApp项目的一部分，遵循项目整体许可证。
