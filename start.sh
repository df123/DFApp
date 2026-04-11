#!/bin/bash
# ========================================
# DFApp 开发环境启动脚本
# ========================================
# 用途：一键启动后端 API、彩票代理服务、前端开发服务器
#
# 使用方法：
#   启动默认服务(后端+前端): ./start.sh
#   启动全部服务(含彩票代理): ./start.sh all
#   停止所有服务: ./start.sh stop
#
# 服务说明：
#   1. 后端 API (DFApp.Web)       - HTTP,  端口 44369
#   2. 彩票代理 (DFApp.LotteryProxy) - HTTP,  端口 5000
#   3. 前端 (DFApp.Vue)           - HTTP,  端口 9949
# ========================================

# 基础路径
BASE_DIR="/home/df/dfapp/DFApp"
VUE_DIR="/home/df/dfapp/DFApp/client"
LOG_DIR="${BASE_DIR}/logs"

# 服务端口
BACKEND_PORT=44369
LOTTERY_PORT=5000
FRONTEND_PORT=9949

# 日志文件
BACKEND_LOG="${LOG_DIR}/backend.log"
LOTTERY_LOG="${LOG_DIR}/lottery-proxy.log"
FRONTEND_LOG="${LOG_DIR}/frontend.log"

# ========================================
# 工具函数
# ========================================

# 打印分隔线
print_separator() {
    echo "========================================"
}

# 检查依赖命令是否存在
check_dependencies() {
    local missing=()
    if ! command -v dotnet &>/dev/null; then
        missing+=("dotnet")
    fi
    if ! command -v pnpm &>/dev/null && ! command -v node &>/dev/null; then
        missing+=("pnpm/node")
    fi
    if ! command -v lsof &>/dev/null; then
        missing+=("lsof")
    fi

    if [ ${#missing[@]} -gt 0 ]; then
        echo "❌ 缺少必要依赖: ${missing[*]}"
        echo "  请先安装以上命令后再运行此脚本"
        exit 1
    fi
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

# 等待端口释放
wait_for_port_release() {
    local port=$1
    local max_wait=10
    local waited=0
    while lsof -ti :${port} &>/dev/null && [ $waited -lt $max_wait ]; do
        sleep 1
        waited=$((waited + 1))
    done
}

# ========================================
# 停止所有服务
# ========================================
stop_all_services() {
    print_separator
    echo "  DFApp 开发环境 - 停止所有服务"
    print_separator

    echo ""
    echo "[1/3] 停止后端 API (端口 ${BACKEND_PORT})..."
    kill_port_processes "$BACKEND_PORT" "后端 API"

    echo ""
    echo "[2/3] 停止彩票代理服务 (端口 ${LOTTERY_PORT})..."
    kill_port_processes "$LOTTERY_PORT" "彩票代理"

    echo ""
    echo "[3/3] 停止前端服务 (端口 ${FRONTEND_PORT})..."
    kill_port_processes "$FRONTEND_PORT" "前端"

    echo ""
    print_separator
    echo "  所有服务已停止"
    print_separator
}

# ========================================
# 启动后端 API
# ========================================
start_backend() {
    local total=$1
    local step=$2

    echo "[${step}/${total}] 启动后端 API (http://0.0.0.0:${BACKEND_PORT})..."
    echo "  - 检查端口 ${BACKEND_PORT}..."
    kill_port_processes "$BACKEND_PORT" "后端 API"
    wait_for_port_release "$BACKEND_PORT"

    echo "  - 启动中..."
    ASPNETCORE_ENVIRONMENT=Development nohup dotnet run --project "${BASE_DIR}/src/DFApp.Web/DFApp.Web.csproj" \
        --urls "http://0.0.0.0:${BACKEND_PORT}" > "${BACKEND_LOG}" 2>&1 &
    disown
    echo "  ✓ 后端 API 已在后台启动"
    echo "  - 日志: ${BACKEND_LOG}"
    echo ""
}

# ========================================
# 启动彩票代理服务
# ========================================
start_lottery() {
    local total=$1
    local step=$2

    echo "[${step}/${total}] 启动彩票代理服务 (http://0.0.0.0:${LOTTERY_PORT})..."
    echo "  - 检查端口 ${LOTTERY_PORT}..."
    kill_port_processes "$LOTTERY_PORT" "彩票代理"
    wait_for_port_release "$LOTTERY_PORT"

    echo "  - 启动中..."
    nohup bash -c "cd '${BASE_DIR}' && ProxySettings__TargetBaseUrl=https://www.cwl.gov.cn ASPNETCORE_ENVIRONMENT=Development dotnet run --project DFApp.LotteryProxy/DFApp.LotteryProxy.csproj --urls 'http://0.0.0.0:${LOTTERY_PORT}'" \
        > "${LOTTERY_LOG}" 2>&1 &
    disown
    echo "  ✓ 彩票代理服务已在后台启动"
    echo "  - 日志: ${LOTTERY_LOG}"
    echo ""
}

# ========================================
# 启动前端服务
# ========================================
start_frontend() {
    local total=$1
    local step=$2

    echo "[${step}/${total}] 启动前端服务 (http://0.0.0.0:${FRONTEND_PORT})..."
    echo "  - 检查端口 ${FRONTEND_PORT}..."
    kill_port_processes "$FRONTEND_PORT" "前端"
    wait_for_port_release "$FRONTEND_PORT"

    echo "  - 启动中..."
    nohup bash -c "cd '${VUE_DIR}' && VITE_PORT=${FRONTEND_PORT} pnpm dev" > "${FRONTEND_LOG}" 2>&1 &
    disown
    echo "  ✓ 前端服务已在后台启动"
    echo "  - 日志: ${FRONTEND_LOG}"
    echo ""
}

# ========================================
# 启动默认服务（后端 + 前端）
# ========================================
start_default_services() {
    local total=2

    print_separator
    echo "  DFApp 开发环境启动脚本"
    print_separator
    echo ""

    # 检查依赖
    check_dependencies

    # 创建日志目录
    mkdir -p "${LOG_DIR}"

    start_backend "$total" 1
    start_frontend "$total" 2

    # ---- 启动完成 ----
    print_separator
    echo "  所有服务已启动！"
    print_separator
    echo "  后端 API:      http://0.0.0.0:${BACKEND_PORT}"
    echo "  前端:          http://0.0.0.0:${FRONTEND_PORT}"
    echo "  Swagger:       http://0.0.0.0:${BACKEND_PORT}/swagger"
    print_separator
    echo "  查看日志: tail -f logs/<service>.log"
    echo "  停止服务: ./start.sh stop"
    echo "  启动全部(含彩票代理): ./start.sh all"
    print_separator
}

# ========================================
# 启动全部服务（后端 + 彩票代理 + 前端）
# ========================================
start_all_services() {
    local total=3

    print_separator
    echo "  DFApp 开发环境启动脚本（全部服务）"
    print_separator
    echo ""

    # 检查依赖
    check_dependencies

    # 创建日志目录
    mkdir -p "${LOG_DIR}"

    start_backend "$total" 1
    start_lottery "$total" 2
    start_frontend "$total" 3

    # ---- 启动完成 ----
    print_separator
    echo "  所有服务已启动！"
    print_separator
    echo "  后端 API:      http://0.0.0.0:${BACKEND_PORT}"
    echo "  彩票代理:      http://0.0.0.0:${LOTTERY_PORT}"
    echo "  前端:          http://0.0.0.0:${FRONTEND_PORT}"
    echo "  Swagger:       http://0.0.0.0:${BACKEND_PORT}/swagger"
    print_separator
    echo "  查看日志: tail -f logs/<service>.log"
    echo "  停止服务: ./start.sh stop"
    print_separator
}

# ========================================
# 主入口
# ========================================

case "${1:-}" in
    stop)
        stop_all_services
        ;;
    all)
        start_all_services
        ;;
    *)
        start_default_services
        ;;
esac
