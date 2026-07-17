# 错误处理与安全响应

## 概述

本文档说明 `DFApp.Web` 的错误响应体系，重点描述如何避免在生产环境泄露 `traceId`、异常堆栈、`System.Text.Json` 等框架细节，以及如何按环境控制模型校验错误的详细程度。

## 统一响应格式

所有错误响应均使用 `ApiResponse<T>`（`Infrastructure/ApiResponse.cs`）：

```json
{
  "success": false,
  "code": "400",
  "message": "请求参数无效，请检查后重试",
  "data": null
}
```

`ApiResponse` 不包含 `traceId`、堆栈、框架类型名等字段，从结构上避免泄露实现细节。

## 三层错误处理

### 1. 全局异常过滤器（`GlobalExceptionFilter`）

- 位置：`Infrastructure/GlobalExceptionFilter.cs`，通过 `AddControllers` 注册为 MVC 过滤器。
- 捕获范围：**控制器执行过程中抛出的异常**（Action 内的业务异常）。
- 行为：
  - `BusinessException` / `ValidationException`：返回业务侧定义的可读消息（面向用户，非框架细节）。
  - `NotFoundException` → 404；`UnauthorizedAccessException` → 401；其他 → 通用“服务器内部错误，请稍后重试”。
- 异常原文仅写入 Serilog 日志，不出现在响应体中。

### 2. 模型校验失败（`ApiErrorResponseFactory.CreateModelStateResponse`）

- 位置：`Infrastructure/ApiErrorResponseFactory.cs`。
- 通过 `ApiBehaviorOptions.InvalidModelStateResponseFactory` 替换 `[ApiController]` 默认返回的 `ValidationProblemDetails`（默认包含 `traceId` 与字段级 `errors` 字典）。
- **环境策略**：
  - **生产环境**：仅返回通用提示 `请求参数无效，请检查后重试`，不暴露具体字段与校验规则。
  - **非生产环境**：返回 `请求参数无效 - {字段: 错误信息}`，便于联调排查。

### 3. 全局异常中间件（`UseExceptionHandler`）

- 位置：`Program.cs` 中间件管道。
- 捕获范围：**MVC 过滤器之外**的异常（如中间件层异常、请求体 JSON 反序列化失败等）。
- 行为：统一返回 `ApiResponse`，HTTP 500，消息 `服务器内部错误，请稍后重试`，不含任何框架细节。
- 配置（`Program.cs`）：

  ```csharp
  if (env.IsDevelopment())
  {
      app.UseDeveloperExceptionPage();
  }
  else
  {
      // 非开发环境统一异常处理，避免泄露 traceId、堆栈及 System.Text.Json 等框架细节
      app.UseExceptionHandler(errorApp => errorApp.Run(ApiErrorResponseFactory.WriteExceptionResponse));
  }
  ```

## 环境差异

| 场景 | 开发/Staging | 生产 |
| --- | --- | --- |
| 未处理异常 | 开发者异常页（详细堆栈） | `ApiResponse` 通用消息（500） |
| 模型校验失败 | 字段级详细错误 | 通用提示（400） |
| 业务异常 | 业务可读消息 | 业务可读消息（保留，面向用户） |
| `traceId` / 框架细节 | 不暴露 | 不暴露 |

## 安全说明

- `ApiResponse` 结构本身不含 `traceId`、堆栈、框架类型等敏感字段。
- 生产环境关闭 `[ApiController]` 默认的详细校验响应与开发者异常页。
- 框架级异常（含 `System.Text.Json` 解析失败）被 `UseExceptionHandler` 统一吞并，仅返回通用消息，细节进入日志。
- 业务异常消息属于面向用户的语义信息（如“用户名已存在”），不属于框架泄露，予以保留。

## 相关文件

- `Infrastructure/ApiResponse.cs` — 统一响应模型
- `Infrastructure/GlobalExceptionFilter.cs` — MVC 异常过滤器
- `Infrastructure/ApiErrorResponseFactory.cs` — 模型校验与全局异常的统一响应工厂
- `Infrastructure/BusinessException.cs` / `ValidationException.cs` — 业务异常类型
- `Program.cs` — 管道与选项配置
