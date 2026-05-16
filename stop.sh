#!/bin/bash
# ========================================
# DFApp 开发环境停止脚本
# ========================================
# 用途：停止后端 API、彩票代理服务、前端开发服务器
#
# 使用方法：
#   停止全部服务:           ./stop.sh
#   停止全部服务(同上):     ./stop.sh all
#   仅停止后端:             ./stop.sh backend
#   仅停止彩票代理:         ./stop.sh lottery
#   仅停止前端:             ./stop.sh frontend
#
# 服务说明：
#   1. 后端 API (DFApp.Web)           - 端口 44369
#   2. 彩票代理 (DFApp.LotteryProxy)  - 端口 5000
#   3. 前端 (DFApp.Vue)               - 端口 9949
# ========================================

# 服务端口
BACKEND_PORT=44369
LOTTERY_PORT=5000
FRONTEND_PORT=9949

# ========================================
# 工具函数
# ========================================

# 打印分隔线
print_separator() {
    echo "========================================"
}

# 安全地杀掉占用指定端口的进程（包括其进程组）
# 排除 VS Code 远程开发相关进程
kill_port_processes() {
    local port=$1
    local service_name=$2
    local pids

    pids=$(lsof -ti :${port} 2>/dev/null) || true

    if [ -z "$pids" ]; then
        echo "  - 端口 ${port} 未被占用"
        return 0
    fi

    echo "  - 发现端口 ${port} 被占用，尝试清理..."

    for pid in $pids; do
        # 获取进程命令行，检查是否为 VS Code 相关进程
        local cmdline
        cmdline=$(tr '\0' ' ' < "/proc/${pid}/cmdline" 2>/dev/null | tr -s ' ') || true

        if echo "$cmdline" | grep -qiE 'vscode|vsc'; then
            echo "  ⚠ 跳过 VS Code 进程 (PID: ${pid})"
            continue
        fi

        # 检查父进程是否为 VS Code 相关进程
        local ppid
        ppid=$(ps -o ppid= -p "$pid" 2>/dev/null | tr -d ' ') || true
        if [ -n "$ppid" ] && [ -f "/proc/${ppid}/cmdline" ]; then
            local pcmdline
            pcmdline=$(tr '\0' ' ' < "/proc/${ppid}/cmdline" 2>/dev/null | tr -s ' ') || true
            if echo "$pcmdline" | grep -qiE 'vscode|vsc'; then
                echo "  ⚠ 跳过进程 (PID: ${pid})，其父进程为 VS Code (PID: ${ppid})"
                continue
            fi
        fi

        # 获取进程组 ID，尝试杀掉整个进程组
        local pgid
        pgid=$(ps -o pgid= -p "$pid" 2>/dev/null | tr -d ' ') || true

        if [ -n "$pgid" ] && [ "$pgid" != "0" ] && [ "$pgid" != "$pid" ]; then
            echo "  - 正在停止 ${service_name} 进程组 (PGID: ${pgid}, 包含 PID: ${pid})..."
            kill -- -"$pgid" 2>/dev/null || true
        else
            echo "  - 正在停止 ${service_name} 进程 (PID: ${pid})..."
            kill "$pid" 2>/dev/null || true
        fi

        sleep 2
        # 如果进程仍在运行，强制杀掉整个进程组
        if kill -0 "$pid" 2>/dev/null; then
            if [ -n "$pgid" ] && [ "$pgid" != "0" ] && [ "$pgid" != "$pid" ]; then
                echo "  - 进程未退出，强制终止进程组 (PGID: ${pgid})..."
                kill -9 -- -"$pgid" 2>/dev/null || true
            else
                echo "  - 进程未退出，强制终止 (PID: ${pid})..."
                kill -9 "$pid" 2>/dev/null || true
            fi
        fi
        echo "  ✓ ${service_name} 已停止"
    done
}

# 清理 PID 文件
cleanup_pid_files() {
    local pid_files=(
        "/tmp/dfapp-backend.pid"
        "/tmp/dfapp-lottery.pid"
        "/tmp/dfapp-frontend.pid"
    )
    local cleaned=false

    for pid_file in "${pid_files[@]}"; do
        if [ -f "$pid_file" ]; then
            # 读取 PID 并确认进程已不存在
            local old_pid
            old_pid=$(cat "$pid_file" 2>/dev/null) || true
            if [ -n "$old_pid" ] && kill -0 "$old_pid" 2>/dev/null; then
                # 进程仍在运行，不由此函数清理
                continue
            fi
            rm -f "$pid_file"
            echo "  - 已清理 PID 文件: ${pid_file}"
            cleaned=true
        fi
    done

    if [ "$cleaned" = false ]; then
        echo "  - 无需清理的 PID 文件"
    fi
}

# ========================================
# 停止单个服务
# ========================================
stop_backend() {
    echo "  停止后端 API (端口 ${BACKEND_PORT})..."
    kill_port_processes "$BACKEND_PORT" "后端 API"
}

stop_lottery() {
    echo "  停止彩票代理服务 (端口 ${LOTTERY_PORT})..."
    kill_port_processes "$LOTTERY_PORT" "彩票代理"
}

stop_frontend() {
    echo "  停止前端服务 (端口 ${FRONTEND_PORT})..."
    kill_port_processes "$FRONTEND_PORT" "前端"
}

# ========================================
# 停止全部服务
# ========================================
stop_all_services() {
    print_separator
    echo "  DFApp 开发环境 - 停止所有服务"
    print_separator

    echo ""
    echo "[1/3]"
    stop_backend

    echo ""
    echo "[2/3]"
    stop_lottery

    echo ""
    echo "[3/3]"
    stop_frontend

    echo ""
    echo "清理 PID 文件..."
    cleanup_pid_files

    echo ""
    print_separator
    echo "  所有服务已停止"
    print_separator
}

# ========================================
# 主入口
# ========================================

# 检查 lsof 是否可用
if ! command -v lsof &>/dev/null; then
    echo "❌ 缺少必要依赖: lsof"
    echo "  请先安装 lsof 后再运行此脚本"
    exit 1
fi

case "${1:-}" in
    backend)
        print_separator
        echo "  DFApp 开发环境 - 停止后端 API"
        print_separator
        echo ""
        stop_backend
        echo ""
        echo "✓ 后端 API 已停止"
        ;;
    lottery)
        print_separator
        echo "  DFApp 开发环境 - 停止彩票代理服务"
        print_separator
        echo ""
        stop_lottery
        echo ""
        echo "✓ 彩票代理服务已停止"
        ;;
    frontend)
        print_separator
        echo "  DFApp 开发环境 - 停止前端服务"
        print_separator
        echo ""
        stop_frontend
        echo ""
        echo "✓ 前端服务已停止"
        ;;
    all|"")
        stop_all_services
        ;;
    *)
        echo "未知参数: $1"
        echo "用法: ./stop.sh [all|backend|lottery|frontend]"
        exit 1
        ;;
esac
