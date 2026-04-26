#!/bin/bash

# Playwright 测试脚本 - 带截图选项

echo "🎯 Playwright 测试运行脚本"
echo "=================================="

# 检查是否在正确的目录
if [ ! -f "package.json" ]; then
    echo "❌ 错误: 请在项目根目录运行此脚本"
    exit 1
fi

# 询问截图选项
echo ""
echo "📸 截图选项:"
echo "1) 仅在失败时保存截图（默认）"
echo "2) 每个测试都保存截图"
echo ""
read -p "请选择 (1-2，默认1): " screenshot_choice
screenshot_choice=${screenshot_choice:-1}

# 询问视频选项
echo ""
echo "🎬 视频录制选项:"
echo "1) 仅在失败时录制（默认）"
echo "2) 每个测试都录制"
echo ""
read -p "请选择 (1-2，默认1): " video_choice
video_choice=${video_choice:-1}

# 根据选择更新配置
case $screenshot_choice in
    1)
        SCREENSHOT_CONFIG="screenshot: \"only-on-failure\""
        echo "✅ 截图: 仅在失败时保存"
        ;;
    2)
        SCREENSHOT_CONFIG="screenshot: \"on\""
        echo "✅ 截图: 每个测试都保存"
        ;;
    *)
        echo "❌ 无效选项，使用默认值"
        SCREENSHOT_CONFIG="screenshot: \"only-on-failure\""
        ;;
esac

case $video_choice in
    1)
        VIDEO_CONFIG="video: \"retain-on-failure\""
        echo "✅ 视频: 仅在失败时录制"
        ;;
    2)
        VIDEO_CONFIG="video: \"on\""
        echo "✅ 视频: 每个测试都录制"
        ;;
    *)
        echo "❌ 无效选项，使用默认值"
        VIDEO_CONFIG="video: \"retain-on-failure\""
        ;;
esac

# 更新 playwright.config.ts
echo ""
echo "📝 更新配置文件..."

sed -i "s/screenshot: \".*\"/${SCREENSHOT_CONFIG}/" playwright.config.ts
sed -i "s/video: \".*\"/${VIDEO_CONFIG}/" playwright.config.ts

echo "✅ 配置已更新"
echo ""

# 运行测试
echo "🚀 运行测试..."
pnpm test

# 显示截图统计
echo ""
echo "📊 截图统计:"
if [ -d "test-results" ]; then
    total_screenshots=$(find test-results -name "*.png" 2>/dev/null | wc -l)
    echo "   总截图数: $total_screenshots"
    
    if [ "$total_screenshots" -gt 0 ]; then
        echo ""
        echo "   按浏览器分类:"
        echo "   - Chromium: $(find test-results -name "*chromium*" -name "*.png" 2>/dev/null | wc -l)"
        echo "   - Firefox: $(find test-results -name "*firefox*" -name "*.png" 2>/dev/null | wc -l)"
        echo "   - Mobile: $(find test-results -name "*Mobile*" -name "*.png" 2>/dev/null | wc -l)"
        
        echo ""
        echo "   存储空间:"
        du -sh test-results/ | cut -f1
    fi
else
    echo "   没有找到 test-results 目录"
fi

echo ""
echo "✅ 完成！"
echo ""
echo "📖 查看截图:"
echo "   find test-results -name \"*.png\" | sort"
echo ""
echo "📖 查看测试报告:"
echo "   pnpm test:report"
