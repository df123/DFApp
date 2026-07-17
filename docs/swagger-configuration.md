# Swagger / OpenAPI 配置

## 概述

项目使用 Swagger（Swashbuckle）提供 API 文档与调试 UI。本文档记录 Swagger 的注册位置及各环境的启用策略。

## 涉及项目

| 项目 | 配置文件 | 说明 |
| --- | --- | --- |
| `DFApp.Web` | `src/DFApp.Web/Program.cs` | 主后端 API |
| `DFApp.LotteryProxy` | `DFApp.LotteryProxy/Program.cs` | 彩票代理服务（端口 5000） |

## 环境启用策略

### DFApp.Web

- `AddSwaggerGen` 服务注册在所有环境生效（仅注册服务，不暴露端点，无安全风险）。
- Swagger **中间件**（`UseSwagger` / `UseSwaggerUI`）仅在 **非生产环境** 注册：

  ```csharp
  // 生产环境关闭 Swagger UI，避免接口结构泄露；仅开发/Staging 等非生产环境启用
  if (!env.IsProduction())
  {
      app.UseSwagger();
      app.UseSwaggerUI(options =>
      {
          options.SwaggerEndpoint("/swagger/v1/swagger.json", "DFApp API");
      });
  }
  ```

- **生产环境**（`ASPNETCORE_ENVIRONMENT=Production`）下访问 `/swagger` 将返回 404，接口结构不会对外暴露。

### DFApp.LotteryProxy

- Swagger 仅在 `IsDevelopment()` 时注册（服务与中间件均条件化）。

## 安全说明

生产环境关闭 Swagger UI 是出于以下考虑：
- 避免接口结构、参数、模型等信息对外泄露，降低被探测攻击的风险。
- Swagger 本身为开发/调试辅助工具，生产环境无业务需求。

若未来生产环境需要临时启用 Swagger（例如对接联调），建议叠加鉴权（HTTP Basic Auth）或 IP 白名单，而不要直接放开。

## 验证

- 部署到生产环境后，访问 `https://<host>/swagger/index.html` 应返回 404。
- 开发/Staging 环境应能正常打开 Swagger UI。
