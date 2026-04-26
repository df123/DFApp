# 彩票数据代理服务器

这是一个轻量级的ASP.NET Core Minimal API项目，用于代理中国福利彩票网站的数据请求，解决境外服务器访问的地理限制问题。

## 功能特性

- 🌐 **代理服务**: 转发HTTP请求到中国福利彩票网站
- 🔒 **IP白名单**: 只允许授权的IP地址访问
- 🔄 **重试机制**: 自动重试失败的请求
- 📝 **详细日志**: 记录所有请求和响应信息
- 🐳 **容器化**: 支持Docker部署
- 📊 **健康检查**: 提供服务状态监控端点

## 项目结构

```
DFApp.LotteryProxy/
├── DFApp.LotteryProxy.csproj    # 项目文件
├── Program.cs                   # 主程序入口
├── appsettings.json             # 生产环境配置
├── appsettings.Development.json # 开发环境配置
├── Dockerfile                   # Docker构建文件
├── docker-compose.yml          # Docker Compose配置
├── .dockerignore               # Docker忽略文件
├── Models/
│   └── ProxySettings.cs        # 配置模型
├── Middleware/
│   └── IpWhitelistMiddleware.cs # IP白名单中间件
├── Services/
│   └── LotteryProxyService.cs  # 代理服务
└── README.md                   # 项目说明
```

## 快速开始

### 1. 配置设置

编辑 `appsettings.json` 文件，配置允许访问的IP地址：

```json
{
  "ProxySettings": {
    "AllowedIPs": [""],
    "TargetBaseUrl": "https://www.cwl.gov.cn",
    "TimeoutSeconds": 30,
    "RetryCount": 3,
    "RetryDelaySeconds": 2
  }
}
```

### 2. 本地运行

```bash
# 克隆项目
git clone <repository-url>
cd DFApp.LotteryProxy

# 还原依赖
dotnet restore

# 运行项目
dotnet run
```

### 3. Docker部署

```bash
# 构建镜像
docker build -t lottery-proxy .

# 运行容器
docker run -d \
  --name lottery-proxy \
  -p 5000:5000 \
  -e ProxySettings__AllowedIPs__0= \
  lottery-proxy
```

### 4. Docker Compose部署

```bash
# 启动服务
docker-compose up -d

# 查看日志
docker-compose logs -f lottery-proxy

# 停止服务
docker-compose down
```

## API接口

### 健康检查

```http
GET /api/health
```

响应示例：
```json
{
  "status": "healthy",
  "timestamp": "2025-11-08T14:00:00Z",
  "version": "1.0.0"
}
```

### 彩票数据代理

```http
GET /api/proxy/lottery/findDrawNotice?name=ssq&dayStart=2025-11-01&dayEnd=2025-11-08&pageNo=1&pageSize=30
```

**参数说明：**
- `name`: 彩票类型 (ssq: 双色球, kl8: 快乐8)
- `dayStart`: 开始日期 (格式: yyyy-MM-dd)
- `dayEnd`: 结束日期 (格式: yyyy-MM-dd)
- `pageNo`: 页码 (从1开始)
- `pageSize`: 每页大小
- 其他原始API支持的参数

## 使用示例

### 美国服务器调用示例

```bash
# 获取双色球最新数据
curl -X GET "http://proxy-server-ip:5000/api/proxy/lottery/findDrawNotice?name=ssq&dayStart=2025-11-01&dayEnd=2025-11-08&pageNo=1&pageSize=30"

# 获取快乐8历史数据
curl -X GET "http://proxy-server-ip:5000/api/proxy/lottery/findDrawNotice?name=kl8&dayStart=2025-11-01&dayEnd=2025-11-08&pageNo=1&pageSize=30"
```

### C# 客户端示例

```csharp
using var httpClient = new HttpClient();
var response = await httpClient.GetAsync(
    "http://proxy-server-ip:5000/api/proxy/lottery/findDrawNotice?name=ssq&dayStart=2025-11-01&dayEnd=2025-11-08&pageNo=1&pageSize=30");

var content = await response.Content.ReadAsStringAsync();
Console.WriteLine(content);
```

## 配置说明

### ProxySettings 配置项

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| AllowedIPs | List<string> | [] | 允许访问的IP地址列表 |
| TargetBaseUrl | string | https://www.cwl.gov.cn | 目标网站基础URL |
| TimeoutSeconds | int | 30 | 请求超时时间（秒） |
| RetryCount | int | 3 | 重试次数 |
| RetryDelaySeconds | int | 2 | 重试延迟时间（秒） |

### 环境变量配置

可以通过环境变量覆盖配置：

```bash
# 设置允许的IP地址
export ProxySettings__AllowedIPs__0=

# 设置超时时间
export ProxySettings__TimeoutSeconds=60

# 设置重试次数
export ProxySettings__RetryCount=5
```

## 安全考虑

1. **IP白名单**: 只允许配置的IP地址访问代理服务
2. **请求限制**: 内置重试机制，避免过度请求目标网站
3. **错误处理**: 不暴露内部错误信息给客户端
4. **日志记录**: 记录所有访问日志，便于监控和审计

## 监控和日志

### 日志配置

项目使用 Serilog 作为日志框架，支持以下日志输出方式：

#### 控制台输出
- 格式：`[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}`
- 包含时间戳、日志级别和详细信息

#### 文件输出
- 路径：`logs/lottery-proxy-.log`
- 滚动策略：按日滚动（每天一个新文件）
- 文件名格式：`lottery-proxy-YYYYMMDD.log`
- 保留策略：保留最近30天的日志文件
- 文件大小限制：单个日志文件最大100MB，超过自动创建新文件
- 格式：`[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}`

#### Docker Compose 日志查看

```bash
# 查看实时日志（包含时间戳）
docker-compose logs -f lottery-proxy

# 查看特定日期的日志文件
docker exec lottery-proxy cat /app/logs/lottery-proxy-20250323.log

# 查看日志目录
docker exec lottery-proxy ls -lh /app/logs
```

#### 日志级别

- `Information`: 记录请求和响应基本信息
- `Warning`: 记录重试和异常情况
- `Error`: 记录严重错误
- `Debug`: 记录详细的调试信息（仅开发环境）

### 日志配置文件

在 `appsettings.json` 中可以自定义日志配置：

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/lottery-proxy-.log",
          "rollingInterval": "Day",
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}",
          "retainedFileCountLimit": 30,
          "rollOnFileSizeLimit": true,
          "fileSizeLimitBytes": 104857600
        }
      }
    ]
  }
}
```

### 健康检查

服务提供健康检查端点 `/api/health`，可以用于负载均衡器和监控系统。

## 故障排除

### 常见问题

1. **403 Forbidden**: 检查客户端IP是否在白名单中
2. **502 Bad Gateway**: 目标网站可能不可用或被反爬虫机制拦截
3. **504 Gateway Timeout**: 请求超时，可以增加超时时间配置

### 调试步骤

1. 检查应用日志
2. 验证IP白名单配置
3. 测试目标网站直接访问
4. 检查网络连接和防火墙设置

## 许可证

本项目采用 MIT 许可证。

## 贡献

欢迎提交 Issue 和 Pull Request 来改进这个项目。

## 联系方式

如有问题或建议，请通过 Issue 联系我们。