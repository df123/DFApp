# 使用官方.NET 10运行时作为基础镜像
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

# 使用.NET 10 SDK作为构建镜像
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 复制项目文件
COPY ["DFApp.LotteryProxy.csproj", "./"]
RUN dotnet restore "./DFApp.LotteryProxy.csproj"

# 复制所有源代码
COPY . .
WORKDIR "/src/."

# 构建应用
RUN dotnet build "DFApp.LotteryProxy.csproj" -c Release -o /app/build

# 发布应用
FROM build AS publish
RUN dotnet publish "DFApp.LotteryProxy.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 构建最终镜像
FROM base AS final
WORKDIR /app

# 安装 curl（用于健康检查）
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# 创建非root用户
RUN useradd -m -s /bin/bash appuser

# 创建日志目录并设置权限
RUN mkdir -p /app/logs && chown -R appuser:appuser /app

# 切换到非root用户
USER appuser

# 复制发布的应用
COPY --from=publish /app/publish .

# 启动应用
ENTRYPOINT ["dotnet", "DFApp.LotteryProxy.dll"]