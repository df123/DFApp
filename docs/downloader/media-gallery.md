# 媒体库（Gallery）功能

2026-08-14 新增。

## 功能

下载队列之外的新页面（`/gallery`，侧边栏"媒体库"入口），用于浏览已下载完成的媒体：

- **图片**：**瀑布流**（多列、图片按原始比例自适应高度不裁剪）展示，点击放大预览（`el-image-viewer` 全屏查看，支持前后切换），显示聊天标题与消息文本；列数随窗口宽度响应式调整（≥1800px 4 列 / ≥1200px 3 列 / ≥700px 2 列 / 其余 1 列），列宽加大后图片显示更大
- **视频**：卡片显示视频缩略图（960 宽高清，按原始比例自适应不裁切）+ "用 VLC 播放"按钮，点击通过 `vlc:` 协议唤起 Windows 侧 VLC 播放本地文件
- 顶部 tab 切换：图片 / 视频 / 全部
- **无分页**：一次性加载全部已完成媒体（瀑布流连续滚动；图片 `loading="lazy"` 懒加载，几十 KB 级缩略图量级可接受），每 30 秒自动刷新

## 聊天标题与消息

- 远程后端 `completed` 接口（`MediaDownloadNotificationDto`）新增 `Message` 字段，随下载通知/补漏同步下发；下载器 `DownloadItem` 实体与 `MediaDownloadNotification` 模型新增 `Message` 列，`EnsureTablesCreated` 对旧库自动补列（text, 可空）
- **历史回填**：旧记录（已完成但 Message 为空）通过 `POST /api/gallery/backfill-messages` 按文件名（即 mediaId）查询远程 `media-info/paged` 接口回填 ChatTitle/Message。实测 272 条历史记录一次回填成功

## VLC 播放（vlc:// 协议）

网页无法直接启动本地桌面程序，采用 Windows 自定义 URL 协议方案：

1. 注册表注册 `vlc:` 协议（HKCU，无需管理员）：
   ```
   HKCU\Software\Classes\vlc
     (Default)      = "URL:VLC Protocol"
     URL Protocol   = ""
   HKCU\Software\Classes\vlc\shell\open\command
     (Default)      = powershell.exe -NoProfile -WindowStyle Hidden -Command "$u=[uri]::UnescapeDataString('%1').Substring(4); Start-Process 'C:\Program Files\VideoLAN\VLC\vlc.exe' -ArgumentList $u"
   ```
   PowerShell 包装的目的：VLC 不识别 `vlc:` 前缀（实测报"无法打开 MRL"），需剥掉前缀后再把路径交给 VLC；`UnescapeDataString` 处理浏览器 URL 编码（空格 → %20 等）。
2. 下载器 `GET /api/gallery` 返回 `windowsPath`（如 `D:\DFApp\xxx.mp4`，WSL 路径 `/mnt/d/...` → Windows 路径转换）
3. 前端按钮：`window.location.href = 'vlc:' + item.windowsPath`

> ⚠️ **必须用 `vlc:`（单冒号）**。`vlc://`（双斜杠）会被浏览器按"带主机名"解析，`D:` 被当作 authority 吃掉盘符冒号、`\` 被规范化为 `/`，VLC 收到相对路径（实测报 `file:///C:/WINDOWS/system32/D%2F/DFApp/...`）。`vlc:` 是 opaque path，浏览器原样传递。

### 部署提醒

`vlc://` 协议注册在 **Windows 注册表**（当前机器 `HKCU\Software\Classes\vlc`），换机器或重装系统需重新执行注册；VLC 路径写死为 `C:\Program Files\VideoLAN\VLC\vlc.exe`，若 VLC 安装在其他位置需同步修改注册表命令。

## 静态文件映射

`Program.cs` 将下载目录映射为 `/media/{文件名}` 静态访问（仅当目录存在时启用），图片卡片直接以此路径加载。`PhysicalFileProvider` 不缓存文件列表，下载完成后即可访问。

## 相关接口

| 接口 | 说明 |
|------|------|
| `GET /api/gallery?page&pageSize` | 已完成媒体分页列表（含 chatTitle/message/mediaUrl/windowsPath） |
| `POST /api/gallery/backfill-messages` | 回填历史记录的聊天标题与消息 |
| `POST /api/gallery/{id}/play` | （备用）后端调用 VLC；前端已改用 vlc:// 协议，此接口保留 |

## 视频缩略图（2026-08-14 新增）

**功能**：媒体库视频卡片显示视频首帧缩略图（不再只有 🎬 占位），点击缩略图同样唤起 VLC 播放。

**实现**：
- **ffmpeg**：静态构建 7.0.2 位于 `~/ffmpeg/ffmpeg`（johnvansickle 官方静态包，无需安装依赖；可用环境变量 `FFMPEG_PATH` 覆盖）。若该机器无 ffmpeg 需重新下载解压到 `~/ffmpeg/` 或配置 `FFMPEG_PATH`。
- **生成**：`DownloadManager.GenerateThumbnailAsync` 用 `ffmpeg -ss 5 -frames:v 1 -vf scale=960:-2` 抽取距片头 5 秒一帧（避开首帧黑屏）为 JPEG（宽 960，比初版 480 更清晰）；已存在则跳过，失败仅记日志不影响下载。
- **时机**：① 视频下载完成时自动生成（`OnDownloadCompleted`）；② 下载器启动时后台批量补齐历史视频（`BackfillThumbnailsAsync`，实测 65 个视频约 1 分钟补齐）。
- **存储**：`{DownloadPath}/thumbs/{视频文件名}.jpg`——注意目录名**不带点**（ASP.NET Core 静态文件中间件默认不提供 `.` 开头的隐藏目录），经 `/media/thumbs/...` 访问。
- **API**：`GET /api/gallery` 返回 `thumbUrl`（缩略图存在时），前端视频卡片 `el-image` 显示；无缩略图时回退 🎬 占位。

## 媒体库删除功能（2026-08-14 新增）

每张卡片右下角"删除"按钮：`ElMessageBox` 确认后调用 `DELETE /api/downloads/{id}`（复用下载队列的 `CancelDownload`），同步删除本地文件与 `.download` 临时文件、DB 记录与分片记录，成功后刷新瀑布流。
