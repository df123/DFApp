# 保存所有测试的截图和视频

## ✅ 配置已启用

现在 Playwright 配置为**每次测试都保存截图和视频**，包括成功的测试。

## 📋 当前配置

```typescript
// playwright.config.ts
use: {
  screenshot: "on",              // 每个测试都保存截图
  video: "on",                   // 每个测试都录制视频
  trace: "on-first-retry",       // 首次重试时保存 trace
}
```

## 📸 截图选项对比

| 选项                | 说明           | 适用场景                 |
| ------------------- | -------------- | ------------------------ |
| `"off"`             | 不保存截图     | 生产环境                 |
| `"only-on-failure"` | 仅失败时保存   | 调试（节省空间）         |
| `"on"`              | 每个测试都保存 | **当前配置（完整记录）** |

## 🎬 视频选项对比

| 选项                  | 说明                     | 适用场景                 |
| --------------------- | ------------------------ | ------------------------ |
| `"off"`               | 不录制视频               | 生产环境                 |
| `"on"`                | 每个测试都录制           | **当前配置（完整记录）** |
| `"retain-on-failure"` | 每个都录制，但删除成功的 | 调试                     |
| `"on-first-retry"`    | 仅首次重试时录制         | 快速测试                 |

## 📊 生成的文件

### 每个测试生成

```
test-results/{test-name}-{browser}/
├── test-finished-1.png    # 截图
├── video.webm              # 视频
└── ...                     # 其他文件（如果失败）
```

### 示例文件

```
test-results/
├── app-Authentication-Tests-should-display-user-information-chromium/
│   ├── test-finished-1.png   (6.5 KB)
│   └── video.webm             (8.5 KB)
├── app-Authentication-Tests-should-navigate-to-lottery-page-firefox/
│   ├── test-finished-1.png   (7.2 KB)
│   └── video.webm             (9.1 KB)
└── ...
```

## 🔍 查看截图和视频

### 查找所有文件

```bash
# 查找所有截图
find test-results -name "*.png"

# 查找所有视频
find test-results -name "*.webm"

# 统计数量
echo "截图: $(find test-results -name "*.png" | wc -l)"
echo "视频: $(find test-results -name "*.webm" | wc -l)"
```

### 按浏览器查看

```bash
# Chromium
find test-results -name "*chromium*" -name "*.png"
find test-results -name "*chromium*" -name "*.webm"

# Firefox
find test-results -name "*firefox*" -name "*.png"
find test-results -name "*firefox*" -name "*.webm"

# Mobile Chrome
find test-results -name "*Mobile*" -name "*.png"
find test-results -name "*Mobile*" -name "*.webm"
```

### 播放视频

```bash
# 使用 ffplay（如果已安装）
ffplay test-results/app-Authentication-Tests-should-display-user-information-chromium/video.webm

# 或使用 vlc
vlc test-results/app-Authentication-Tests-should-display-user-information-chromium/video.webm

# 或使用浏览器
xdg-open test-results/app-Authentication-Tests-should-display-user-information-chromium/video.webm
```

### 查看截图

```bash
# 使用图片查看器
eog test-results/*/*.png

# 或使用 feh
feh test-results/*/*.png

# 或打开文件夹
xdg-open test-results/
```

## 📈 统计信息

```bash
# 查看文件统计
cd test-results

echo "测试结果统计:"
echo "============="
echo "测试目录数: $(find . -type d | wc -l)"
echo "截图数量: $(find . -name "*.png" | wc -l)"
echo "视频数量: $(find . -name "*.webm" | wc -l)"

echo ""
echo "存储空间:"
du -sh .
du -sh ./*/*.png 2>/dev/null | tail -5
du -sh ./*/*.webm 2>/dev/null | tail -5
```

## 🗑️ 清理策略

### 仅保留最近 7 天的文件

```bash
find test-results -name "*.png" -mtime +7 -delete
find test-results -name "*.webm" -mtime +7 -delete
```

### 仅保留成功的测试结果

如果只想保存成功测试的截图和视频：

```bash
# 删除失败测试的截图
find test-results -name "test-failed*.png" -delete

# 删除失败测试的视频
find test-results -path "*/test-failed*" -name "*.webm" -delete
```

### 删除所有视频

如果不需要视频（只保留截图）：

```bash
find test-results -name "*.webm" -delete
```

## ⚙️ 配置修改

### 改为仅失败时保存

如果不需要所有测试的截图和视频，修改 `playwright.config.ts`：

```typescript
use: {
  screenshot: "only-on-failure",  // 仅失败时截图
  video: "retain-on-failure",     // 仅失败时保留视频
}
```

### 完全禁用

```typescript
use: {
  screenshot: "off",  // 不截图
  video: "off",       // 不录制
}
```

## 💾 存储空间管理

### 估算存储空间

- **截图**: 平均 5-10 KB/个
- **视频**: 平均 5-15 KB/个
- **每个测试**: 约 10-25 KB

### 100 个测试估算

- **截图**: 500 KB - 1 MB
- **视频**: 500 KB - 1.5 MB
- **总计**: 1 - 2.5 MB

## 📊 测试报告

### 查看 HTML 报告

```bash
pnpm test:report
```

HTML 报告中会显示每个测试的截图缩略图。

### 使用 Playwright Inspector

```bash
# 如果有 trace 文件
npx playwright show-trace test-results/{test-name}-{browser}/trace.zip
```

## 🔧 高级配置

### 调整视频质量

```typescript
use: {
  video: {
    mode: "on",
    size: {
      width: 1280,
      height: 720
    }
  }
}
```

### 调整截图质量

```typescript
use: {
  screenshot: {
    mode: "on",
    fullPage: false  // 仅视口，不是完整页面
  }
}
```

### 自定义截图路径

```typescript
use: {
  screenshot: {
    mode: "on",
    path: "screenshots/"  // 自定义路径
  }
}
```

## 📌 注意事项

1. **磁盘空间**: 启用所有截图和视频会占用更多磁盘空间
2. **性能影响**: 录制视频会稍微降低测试速度
3. **CI/CD**: 在 CI 环境中，考虑使用 `"only-on-failure"` 以节省空间
4. **定期清理**: 建议定期清理旧的测试结果
5. **敏感信息**: 截图可能包含敏感信息，注意保护

## 🎯 使用场景

### 适合使用 `"on"` 的场景

- ✅ 需要完整的测试记录
- ✅ 用于测试报告
- ✅ 调试问题
- ✅ 演示测试过程
- ✅ 文档记录

### 适合使用 `"only-on-failure"` 的场景

- ✅ 日常开发测试
- ✅ 节省磁盘空间
- ✅ 快速迭代
- ✅ CI/CD 管道

## 🔗 相关文档

- [Playwright 截图文档](https://playwright.dev/docs/screenshots)
- [Playwright 视频文档](https://playwright.dev/docs/videos)
- [Playwright Trace 文档](https://playwright.dev/docs/trace-viewer)
- [测试快速开始](./TESTING.md)
