# 下载器速度记录：仪表盘查看不同时间段的下载速度

> 适用模块：`DFApp.Downloader`
> 创建日期：2026-08-20

## 一、背景

仪表盘的"下载速度"卡片取自 `DownloadManager._activeSpeeds`（内存瞬时值，下载结束即清除），只能看当前时刻，无法回顾不同时间段的速度情况。

## 二、方案

后台周期采样全局总下载速度并落库，仪表盘新增"速度记录"折线图，按时间范围切换查看。

### 数据层（DFApp.Downloader.Core）

- 新实体 `Entities/DownloadSpeedSample.cs`，表 `DownloadSpeedSamples`：
  - `RecordedAt`：采样时间（UTC）
  - `SpeedBytesPerSecond`：采样时刻的全局总速度（各活跃下载瞬时速度之和）
  - `RecordedAt` 建索引；建表由 `DownloaderDbContext.EnsureTablesCreated()` CodeFirst 自动完成
- `DownloadManager` 新增后台采样循环 `SpeedSamplerAsync`（随 `StartAsync` 启动，与卡死看门狗同模式）：
  - 每 **1 分钟**采样一次，仅在有活跃下载（`_activeSpeeds` 非空）时写入样本；空闲期不采样，即视为 0 速度
  - 每小时清理超过 **30 天**的旧样本，防止表无限增长

### API（DFApp.Downloader.App）

`DownloadsController` 新增端点：

| 方法 | 路由 | 说明 |
|------|------|------|
| GET | `/api/speed-history?range=24h` | 速度记录，range 取值 `1h / 6h / 24h / 7d / 30d`，默认 `24h` |

range 与聚合桶粒度对应关系：

| range | 时间跨度 | 桶粒度 |
|-------|---------|--------|
| 1h | 最近 1 小时 | 1 分钟 |
| 6h | 最近 6 小时 | 5 分钟 |
| 24h | 最近 24 小时 | 15 分钟 |
| 7d | 最近 7 天 | 1 小时 |
| 30d | 最近 30 天 | 3 小时 |

返回结构（桶内无样本即空闲期，速度为 0）：

```json
{
  "range": "24h",
  "bucketSeconds": 900,
  "items": [ { "time": "2026-08-20T10:00:00Z", "avgSpeed": 1234567.8, "maxSpeed": 2345678.9 } ]
}
```

`time` 为桶起始时间（UTC，序列化带 Z 后缀）；`avgSpeed`/`maxSpeed` 为桶内样本的平均值与峰值（字节/秒）。

### 前端（DFApp.Downloader/web）

- 新增依赖 `echarts`（与主前端一致）。
- `api/downloader.ts` 新增 `SpeedHistory` 类型与 `getSpeedHistory(range)`。
- `Dashboard.vue` 统计卡片下方新增全宽"速度记录"卡片：
  - 头部 `el-radio-group` 切换时间范围（最近1小时/6小时/24小时/7天/30天）
  - echarts 折线图：平均速度（面积折线）+ 峰值速度（虚线），Y 轴速度自适应单位，tooltip 显示格式化速度
  - 每 60 秒自动刷新，切换范围即时加载，组件卸载时释放图表与 resize 监听

## 三、数据保留

采样间隔 1 分钟、保留 30 天，最大约 4.3 万行（每行仅两个字段），由采样循环自动清理，无需人工维护。
