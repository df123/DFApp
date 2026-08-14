# 媒体库（Gallery）功能

2026-08-14 新增。

## 功能

下载队列之外的新页面（`/gallery`，侧边栏"媒体库"入口），用于浏览已下载完成的媒体：

- **图片**：卡片网格展示，点击可放大预览（`el-image` 预览组，支持前后切换），显示聊天标题与消息文本
- **视频**：卡片显示 🎬 占位 + "用 VLC 播放"按钮，点击通过 `vlc://` 协议唤起 Windows 侧 VLC 播放本地文件
- 顶部 tab 切换：图片 / 视频 / 全部
- 分页展示（每页 60 条），每 30 秒自动刷新

## 聊天标题与消息

- 远程后端 `completed` 接口（`MediaDownloadNotificationDto`）新增 `Message` 字段，随下载通知/补漏同步下发；下载器 `DownloadItem` 实体与 `MediaDownloadNotification` 模型新增 `Message` 列，`EnsureTablesCreated` 对旧库自动补列（text, 可空）
- **历史回填**：旧记录（已完成但 Message 为空）通过 `POST /api/gallery/backfill-messages` 按文件名（即 mediaId）查询远程 `media-info/paged` 接口回填 ChatTitle/Message。实测 272 条历史记录一次回填成功

## VLC 播放（vlc:// 协议）

网页无法直接启动本地桌面程序，采用 Windows 自定义 URL 协议方案：

1. 注册表注册 `vlc://` 协议（HKCU，无需管理员）：
   ```
   HKCU\Software\Classes\vlc
     (Default)      = "URL:VLC Protocol"
     URL Protocol   = ""
   HKCU\Software\Classes\vlc\shell\open\command
     (Default)      = powershell.exe -NoProfile -WindowStyle Hidden -Command "$u=[uri]::UnescapeDataString('%1').Substring(6); Start-Process 'C:\Program Files\VideoLAN\VLC\vlc.exe' -ArgumentList $u"
   ```
   PowerShell 包装的目的：VLC 不识别 `vlc://` 前缀（实测报"无法打开 MRL"），需剥掉前缀后再把路径交给 VLC；`UnescapeDataString` 处理浏览器 URL 编码（空格 → %20 等）。
2. 下载器 `GET /api/gallery` 返回 `windowsPath`（如 `D:\DFApp\xxx.mp4`，WSL 路径 `/mnt/d/...` → Windows 路径转换）
3. 前端按钮：`window.location.href = 'vlc://' + item.windowsPath`

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
