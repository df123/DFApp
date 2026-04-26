#!/bin/bash

# Playwright 配置验证脚本

echo "🔍 Playwright 配置验证"
echo "=================================="

# 检查项目结构
echo ""
echo "📁 检查项目结构..."

files_to_check=(
    "playwright.config.ts"
    "tests/auth.setup.ts"
    "tests/app.spec.ts"
    "tests/e2e.spec.ts"
    "tests/navigation.spec.ts"
    "tests/features.spec.ts"
    "docs/playwright-testing.md"
    "docs/playwright-quick-reference.md"
    "docs/playwright-summary.md"
    "TESTING.md"
    "playwright/.auth/.gitkeep"
    "scripts/run-playwright-tests.sh"
)

all_files_exist=true
for file in "${files_to_check[@]}"; do
    if [ -f "$file" ]; then
        echo "✅ $file"
    else
        echo "❌ $file (缺失)"
        all_files_exist=false
    fi
done

# 检查 package.json 中的脚本
echo ""
echo "📝 检查 package.json 脚本..."

if grep -q '"test": "playwright test"' package.json; then
    echo "✅ pnpm test"
else
    echo "❌ pnpm test (缺失)"
    all_files_exist=false
fi

if grep -q '"test:ui": "playwright test --ui"' package.json; then
    echo "✅ pnpm test:ui"
else
    echo "❌ pnpm test:ui (缺失)"
    all_files_exist=false
fi

if grep -q '"test:install": "playwright install --with-deps"' package.json; then
    echo "✅ pnpm test:install"
else
    echo "❌ pnpm test:install (缺失)"
    all_files_exist=false
fi

# 检查 .gitignore
echo ""
echo "🚫 检查 .gitignore..."

if grep -q "playwright-report/" .gitignore; then
    echo "✅ playwright-report/ 已忽略"
else
    echo "❌ playwright-report/ 未忽略"
    all_files_exist=false
fi

if grep -q "test-results/" .gitignore; then
    echo "✅ test-results/ 已忽略"
else
    echo "❌ test-results/ 未忽略"
    all_files_exist=false
fi

if grep -q "playwright/.auth/\*.json" .gitignore; then
    echo "✅ playwright/.auth/*.json 已忽略"
else
    echo "❌ playwright/.auth/*.json 未忽略"
    all_files_exist=false
fi

# 检查后端服务
echo ""
echo "🔗 检查后端服务..."

if curl -s -o /dev/null -w "%{http_code}" https://localhost:44369/api/abp/application-configuration 2>/dev/null | grep -q "200\|401"; then
    echo "✅ 后端服务正在运行"
else
    echo "⚠️  后端服务未运行或不可访问"
    echo "   请启动后端服务: cd /home/df/dfapp/DFApp && dotnet run"
fi

# 检查 Playwright 浏览器
echo ""
echo "🌐 检查 Playwright 浏览器..."

if [ -d "node_modules/playwright" ]; then
    echo "✅ Playwright 已安装"
else
    echo "❌ Playwright 未安装"
    echo "   请运行: pnpm install && pnpm test:install"
    all_files_exist=false
fi

# 检查测试用户配置
echo ""
echo "👤 检查测试用户配置..."

echo "   测试用户配置:"
echo "   - 用户名: test"
echo "   - 密码: 1q2w3E*"
echo "   - 角色: Admin"
echo ""
echo "   请确保后端已创建此用户，参考: /home/df/dfapp/DFApp/docs/backend-testing-config.md"

# 总结
echo ""
echo "=================================="
if [ "$all_files_exist" = true ]; then
    echo "✅ 所有文件和配置检查通过！"
    echo ""
    echo "🚀 快速开始:"
    echo "   1. 运行测试: pnpm test"
    echo "   2. UI 模式: pnpm test:ui"
    echo "   3. 查看文档: cat TESTING.md"
    echo "   4. 使用脚本: ./scripts/run-playwright-tests.sh"
else
    echo "❌ 部分文件或配置缺失，请检查上述错误"
    exit 1
fi
