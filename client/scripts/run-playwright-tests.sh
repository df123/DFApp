#!/bin/bash

# Playwright 测试启动脚本

echo "🚀 Playwright 测试启动脚本"
echo "================================="

# 检查是否在正确的目录
if [ ! -f "package.json" ]; then
    echo "❌ 错误: 请在项目根目录运行此脚本"
    exit 1
fi

# 检查是否安装了 playwright
if ! command -v npx &> /dev/null; then
    echo "❌ 错误: 未找到 npx，请先安装 Node.js"
    exit 1
fi

# 检查后端服务是否运行
echo "🔍 检查后端服务..."
if ! curl -s -o /dev/null -w "%{http_code}" https://localhost:44369/api/abp/application-configuration | grep -q "200\|401"; then
    echo "⚠️  警告: 后端服务未运行或不可访问"
    echo "请先启动后端服务："
    echo "  cd /home/df/dfapp/DFApp"
    echo "  dotnet run"
    echo ""
    read -p "是否继续运行测试？(y/N) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        exit 1
    fi
fi

# 检查 playwright 浏览器
echo "🔍 检查 Playwright 浏览器..."
if [ ! -d "node_modules/playwright" ]; then
    echo "📦 正在安装 Playwright 浏览器..."
    pnpm test:install
fi

# 询问运行模式
echo ""
echo "请选择运行模式："
echo "1) 运行所有测试 (headless)"
echo "2) 运行测试 UI 模式"
echo "3) 查看测试报告"
echo "4) 调试模式"
echo ""
read -p "请输入选项 (1-4): " choice

case $choice in
    1)
        echo "🧪 运行所有测试..."
        pnpm test
        ;;
    2)
        echo "🎨 启动测试 UI 模式..."
        pnpm test:ui
        ;;
    3)
        echo "📊 打开测试报告..."
        pnpm test:report
        ;;
    4)
        echo "🐛 调试模式..."
        pnpm test --debug
        ;;
    *)
        echo "❌ 无效选项"
        exit 1
        ;;
esac

echo ""
echo "✅ 完成！"
