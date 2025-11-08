# 使用官方.NET 8运行时作为基础镜像
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000

# 使用.NET 9 SDK作为构建镜像
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
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

# 创建非root用户
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# 复制发布的应用
COPY --from=publish /app/publish .

# 设置健康检查
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5000/api/health || exit 1

# 启动应用
ENTRYPOINT ["dotnet", "DFApp.LotteryProxy.dll"]