# 启用测试截图

## 方法 1: 使用脚本（推荐）

```bash
./scripts/run-with-screenshots.sh
```

脚本会提示你选择：

1. 截图选项：仅失败时 vs 每个测试
2. 视频选项：仅失败时 vs 每个测试

## 方法 2: 手动修改配置

编辑 `playwright.config.ts`：

```typescript
use: {
  screenshot: "on",              // 改为 "on" 保存所有截图
  video: "on",                   // 改为 "on" 录制所有视频
  // ...
}
```

## 方法 3: 环境变量

```bash
# 运行时设置环境变量
SCREENSHOT="on" pnpm test
```

## 查看截图

### 查找所有截图

```bash
find test-results -name "*.png" | sort
```

### 按浏览器查看

```bash
# Chromium
find test-results -name "*chromium*" -name "*.png"

# Firefox
find test-results -name "*firefox*" -name "*.png"

# Mobile
find test-results -name "*Mobile*" -name "*.png"
```

### 查看截图数量

```bash
find test-results -name "*.png" | wc -l
```

### 打开所有截图

```bash
# Linux
eog test-results/*/*.png

# macOS
open test-results/*/*.png
```

## 配置对比

| 配置    | 截图   | 视频   | 说明     |
| ------- | ------ | ------ | -------- |
| 默认    | 失败时 | 失败时 | 节省空间 |
| `"on"`  | 所有   | 所有   | 完整记录 |
| `"off"` | 无     | 无     | 最快速度 |

## 注意事项

1. **存储空间**：启用所有截图会占用较多磁盘空间
2. **性能影响**：截图会稍微降低测试速度
3. **CI/CD**：建议在 CI 中使用 `"only-on-failure"`
4. **定期清理**：定期删除旧的测试结果

## 快速命令

```bash
# 启用所有截图并运行
sed -i 's/screenshot: "only-on-failure"/screenshot: "on"/' playwright.config.ts
pnpm test

# 恢复默认配置
sed -i 's/screenshot: "on"/screenshot: "only-on-failure"/' playwright.config.ts

# 清理截图
rm -rf test-results/*
```
