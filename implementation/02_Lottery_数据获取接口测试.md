# 彩票数据获取接口测试

## 概述

为了解决 `LotteryResultTimer` 中 `GetLotteryResult` 方法无法抓取数据的问题，我们创建了一个可以手动调用的接口，并添加了详细的日志记录功能。

## 新增文件

### 后端文件

1. **src/DFApp.Application.Contracts/Lottery/LotteryDataFetchDto.cs**
   - 定义了彩票数据获取的请求和响应DTO

2. **src/DFApp.Application.Contracts/Lottery/ILotteryDataFetchService.cs**
   - 定义了彩票数据获取服务的接口

3. **src/DFApp.Application/Lottery/LotteryDataFetchService.cs**
   - 实现了彩票数据获取服务，包含手动调用接口

### 前端文件

1. **DFApp.Vue/src/api/lotteryDataFetch.ts**
   - 前端API调用封装

2. **DFApp.Vue/src/views/lottery/data-fetch.vue**
   - 数据获取测试页面

3. **DFApp.Vue/src/router/modules/lottery.ts**
   - 添加了数据获取测试页面的路由配置

## 修改的文件

1. **src/DFApp.Application/Background/LotteryResultTimer.cs**
   - 添加了详细的日志记录，便于排查问题

## 接口说明

### 1. 手动获取彩票数据

**接口地址**: `POST /api/app/lottery-data-fetch/fetch-lottery-data`

**请求参数**:
```json
{
  "dayStart": "2023-01-01",    // 开始日期
  "dayEnd": "2023-01-31",      // 结束日期
  "pageNo": 1,                 // 页码
  "lotteryType": "ssq",         // 彩票类型: ssq(双色球) 或 kl8(快乐8)
  "saveToDatabase": false         // 是否保存到数据库
}
```

**响应结果**:
```json
{
  "success": true,
  "message": "成功获取到 10 条数据",
  "data": { /* 彩票数据 */ },
  "savedCount": 0,
  "requestUrl": "https://www.cwl.gov.cn/...",
  "statusCode": 200,
  "responseTime": 1500
}
```

### 2. 获取双色球最新数据

**接口地址**: `GET /api/app/lottery-data-fetch/fetch-ssq-latest-data`

**响应结果**: 同上，但自动设置为今天的数据和双色球类型

### 3. 获取快乐8最新数据

**接口地址**: `GET /api/app/lottery-data-fetch/fetch-kl8-latest-data`

**响应结果**: 同上，但自动设置为今天的数据和快乐8类型

### 4. 测试API连接

**接口地址**: `GET /api/app/lottery-data-fetch/test-lottery-api-connection`

**请求参数**:
- `lotteryType`: 彩票类型，默认为 "ssq"

**响应结果**: 同上，但不会保存数据到数据库

## 使用方法

### 1. 通过前端页面测试

1. 启动前端项目
2. 导航到 "彩票管理" -> "数据获取测试"
3. 使用页面上的按钮进行测试：
   - 获取双色球最新数据
   - 获取快乐8最新数据
   - 测试API连接
   - 自定义查询

### 2. 直接调用API

可以使用 Postman 或其他API工具直接调用上述接口。

## 日志记录

### 后端日志

所有操作都会在后端生成详细的日志，包括：

1. **请求信息**:
   - 彩票类型
   - 日期范围
   - 页码
   - 请求URL

2. **HTTP响应信息**:
   - 状态码
   - 响应内容长度
   - 响应内容（前500字符）

3. **数据处理信息**:
   - 反序列化结果
   - 数据条数
   - 第一条数据的详细信息

4. **错误信息**:
   - HTTP请求异常
   - JSON解析异常
   - 数据库操作异常

### 前端日志

前端页面会显示每次操作的详细结果，包括：

1. **操作信息**:
   - 操作时间
   - 操作类型
   - 彩票类型

2. **结果信息**:
   - 是否成功
   - 消息内容
   - 数据条数
   - 保存条数
   - 响应时间
   - HTTP状态码

3. **详细信息**:
   - 请求URL
   - 数据预览

## 排查问题的步骤

1. **测试API连接**:
   - 使用 "测试API连接" 功能检查网络连接是否正常

2. **检查日志**:
   - 查看后端日志，确认请求是否正常发送
   - 检查HTTP响应状态码和内容

3. **验证数据格式**:
   - 检查返回的JSON格式是否正确
   - 确认数据结构是否符合预期

4. **测试不同参数**:
   - 尝试不同的日期范围
   - 测试不同的彩票类型
   - 调整页码参数

5. **检查数据库连接**:
   - 如果需要保存数据，确认数据库连接是否正常

## 注意事项

1. **API限制**:
   - 中国福利彩票官网可能有访问频率限制
   - 建议不要频繁调用接口

2. **数据保存**:
   - 只有在明确设置 `saveToDatabase: true` 时才会保存数据
   - 测试时建议先不保存数据

3. **错误处理**:
   - 所有接口都有完善的错误处理
   - 错误信息会记录在日志中

4. **性能考虑**:
   - 大量数据获取可能需要较长时间
   - 建议分批获取历史数据