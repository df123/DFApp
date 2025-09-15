# Implementation Plan

## [Overview]
将 src\DFApp.Web\VueApp 和 src\DFApp.Web\Pages 的所有页面迁移到 DFApp.Vue 项目，实现前后端完全分离的架构。

本次迁移将把基于 ABP Framework 的 Razor Pages 应用转换为现代化的 Vue 3 单页应用。迁移包括将现有的 VueApp 组件整合到 DFApp.Vue 项目中，将所有 Razor Pages 重构为 Vue 组件，实现标准的 HTTP API 调用方式，集成 ABP OpenIddict 认证系统，并最终实现独立的前端应用部署。这将提升用户体验，改善代码维护性，并为未来的功能扩展提供更好的技术基础。

## [Types]
定义迁移过程中需要的类型定义和接口规范。

### API 响应类型
```typescript
// API 基础响应类型
interface ApiResponse<T = any> {
  success: boolean;
  result: T;
  error?: {
    code: string;
    message: string;
    details?: string;
  };
}

// 分页响应类型
interface PagedResult<T> {
  items: T[];
  totalCount: number;
}

// 认证相关类型
interface LoginRequest {
  userNameOrEmailAddress: string;
  password: string;
  rememberMe?: boolean;
}

interface TokenResponse {
  access_token: string;
  token_type: string;
  expires_in: number;
  refresh_token?: string;
}

interface UserInfo {
  id: string;
  userName: string;
  email: string;
  roles: string[];
  permissions: string[];
}
```

### 业务实体类型
```typescript
// 现有 DTO 类型迁移
interface LotteryDto {
  id: string;
  name: string;
  description: string;
  // ... 其他字段
}

interface ExpenditureDto {
  id: string;
  amount: number;
  category: string;
  date: string;
  isBelongToSelf: boolean;
  // ... 其他字段
}

interface ConfigurationDto {
  name: string;
  value: string;
  displayName: string;
  description: string;
}
```

## [Files]
详细说明需要创建、修改和删除的文件。

### 新建文件
- `DFApp.Vue/src/api/` - API 调用模块
  - `DFApp.Vue/src/api/auth.ts` - 认证相关 API
  - `DFApp.Vue/src/api/lottery.ts` - 彩票模块 API
  - `DFApp.Vue/src/api/bookkeeping.ts` - 记账模块 API
  - `DFApp.Vue/src/api/configuration.ts` - 配置模块 API
  - `DFApp.Vue/src/api/aria2.ts` - Aria2 模块 API
  - `DFApp.Vue/src/api/fileUpload.ts` - 文件上传 API
  - `DFApp.Vue/src/api/dynamicIp.ts` - 动态IP API
  - `DFApp.Vue/src/api/logViewer.ts` - 日志查看 API
  - `DFApp.Vue/src/api/telegram.ts` - Telegram 模块 API

- `DFApp.Vue/src/views/` - 页面组件
  - `DFApp.Vue/src/views/home/index.vue` - 首页
  - `DFApp.Vue/src/views/aria2/index.vue` - Aria2 管理页面
  - `DFApp.Vue/src/views/bookkeeping/` - 记账模块页面
  - `DFApp.Vue/src/views/configuration/index.vue` - 配置管理页面
  - `DFApp.Vue/src/views/dynamicIp/index.vue` - 动态IP页面
  - `DFApp.Vue/src/views/fileUpload/index.vue` - 文件上传页面
  - `DFApp.Vue/src/views/logViewer/index.vue` - 日志查看页面
  - `DFApp.Vue/src/views/lottery/` - 彩票模块页面
  - `DFApp.Vue/src/views/telegram/` - Telegram 模块页面

- `DFApp.Vue/src/utils/auth.ts` - 认证工具类
- `DFApp.Vue/src/utils/http.ts` - HTTP 请求封装
- `DFApp.Vue/src/store/modules/auth.ts` - 认证状态管理
- `DFApp.Vue/src/router/modules/` - 路由模块定义

### 迁移文件
- `src/DFApp.Web/VueApp/src/` 下的所有组件迁移到 `DFApp.Vue/src/components/`
- 现有 DTO 类型定义迁移到 `DFApp.Vue/src/types/`

### 修改文件
- `DFApp.Vue/package.json` - 添加必要依赖
- `DFApp.Vue/src/router/index.ts` - 更新路由配置
- `DFApp.Vue/src/store/index.ts` - 添加新的状态模块
- `DFApp.Vue/vite.config.ts` - 配置代理和构建选项
- `DFApp.Vue/.env.development` - 开发环境配置
- `DFApp.Vue/.env.production` - 生产环境配置

### 删除文件（迁移完成后）
- `src/DFApp.Web/VueApp/` - 整个目录
- `src/DFApp.Web/Pages/` - 除了认证相关的页面
- `src/DFApp.Web/package.json` - Vue 相关依赖

## [Functions]
详细说明需要创建和修改的函数。

### 新建函数

#### 认证相关函数 (`DFApp.Vue/src/utils/auth.ts`)
```typescript
// 获取访问令牌
function getAccessToken(): string | null

// 设置访问令牌
function setAccessToken(token: string): void

// 清除认证信息
function clearAuth(): void

// 检查是否已认证
function isAuthenticated(): boolean

// 刷新令牌
async function refreshToken(): Promise<boolean>
```

#### HTTP 请求函数 (`DFApp.Vue/src/utils/http.ts`)
```typescript
// 创建 axios 实例
function createHttpClient(): AxiosInstance

// GET 请求
async function get<T>(url: string, params?: any): Promise<ApiResponse<T>>

// POST 请求
async function post<T>(url: string, data?: any): Promise<ApiResponse<T>>

// PUT 请求
async function put<T>(url: string, data?: any): Promise<ApiResponse<T>>

// DELETE 请求
async function del<T>(url: string): Promise<ApiResponse<T>>
```

#### API 调用函数
每个模块的 API 文件中包含对应的 CRUD 操作函数：
```typescript
// 示例：lottery.ts
async function getLotteries(params: GetLotteriesInput): Promise<PagedResult<LotteryDto>>
async function createLottery(input: CreateLotteryDto): Promise<LotteryDto>
async function updateLottery(id: string, input: UpdateLotteryDto): Promise<LotteryDto>
async function deleteLottery(id: string): Promise<void>
```

### 修改函数

#### 路由守卫函数 (`DFApp.Vue/src/router/index.ts`)
```typescript
// 修改现有的路由守卫，添加权限检查
router.beforeEach(async (to, from, next) => {
  // 添加认证检查
  // 添加权限验证
  // 处理令牌刷新
})
```

#### 状态管理函数 (`DFApp.Vue/src/store/modules/auth.ts`)
```typescript
// 登录 action
async function login(credentials: LoginRequest): Promise<void>

// 登出 action
async function logout(): Promise<void>

// 获取用户信息 action
async function getUserInfo(): Promise<void>
```

## [Classes]
详细说明需要创建和修改的类。

### 新建类

#### HTTP 客户端类 (`DFApp.Vue/src/utils/http.ts`)
```typescript
class HttpClient {
  private instance: AxiosInstance;
  
  constructor(baseURL: string) {
    // 初始化 axios 实例
    // 设置请求拦截器
    // 设置响应拦截器
  }
  
  async request<T>(config: AxiosRequestConfig): Promise<ApiResponse<T>>
  async get<T>(url: string, params?: any): Promise<ApiResponse<T>>
  async post<T>(url: string, data?: any): Promise<ApiResponse<T>>
  async put<T>(url: string, data?: any): Promise<ApiResponse<T>>
  async delete<T>(url: string): Promise<ApiResponse<T>>
}
```

#### 认证管理类 (`DFApp.Vue/src/utils/auth.ts`)
```typescript
class AuthManager {
  private tokenKey: string = 'access_token';
  private refreshTokenKey: string = 'refresh_token';
  
  getToken(): string | null
  setToken(token: string): void
  clearToken(): void
  isAuthenticated(): boolean
  async refreshToken(): Promise<boolean>
}
```

#### API 服务基类 (`DFApp.Vue/src/api/base.ts`)
```typescript
abstract class BaseApiService {
  protected http: HttpClient;
  protected baseUrl: string;
  
  constructor(baseUrl: string) {
    this.baseUrl = baseUrl;
    this.http = new HttpClient(baseUrl);
  }
  
  protected buildUrl(endpoint: string): string
}
```

### 修改类

#### 现有组件类重构
将现有的 VueApp 组件从 Options API 重构为 Composition API：
```typescript
// 示例：支出分析组件
export default defineComponent({
  name: 'ExpenditureAnalysis',
  setup() {
    // 使用 Composition API 重构
    // 添加响应式数据
    // 添加计算属性
    // 添加方法
    // 添加生命周期钩子
  }
})
```

## [Dependencies]
详细说明依赖包的添加和修改。

### 新增依赖包
```json
{
  "dependencies": {
    "axios": "^1.11.0",
    "oidc-client-ts": "^3.0.1",
    "@microsoft/signalr": "^8.0.0",
    "chart.js": "^4.4.0",
    "crypto-js": "^4.2.0"
  },
  "devDependencies": {
    "@types/crypto-js": "^4.2.0"
  }
}
```

### 依赖说明
- `axios` - HTTP 客户端，替代直接的 ABP 服务调用
- `oidc-client-ts` - OpenID Connect 客户端，用于集成 ABP OpenIddict 认证
- `@microsoft/signalr` - SignalR 客户端，用于实时通信功能
- `chart.js` - 图表库，用于数据可视化
- `crypto-js` - 加密库，用于数据加密处理

### 配置更新
更新 `vite.config.ts` 配置：
```typescript
export default defineConfig({
  server: {
    proxy: {
      '/api': {
        target: 'https://localhost:44350',
        changeOrigin: true,
        secure: false
      }
    }
  }
})
```

## [Testing]
详细说明测试策略和测试文件要求。

### 测试文件结构
```
DFApp.Vue/tests/
├── unit/
│   ├── components/
│   ├── utils/
│   └── api/
├── integration/
│   ├── auth/
│   └── api/
└── e2e/
    ├── login.spec.ts
    ├── lottery.spec.ts
    └── bookkeeping.spec.ts
```

### 单元测试
- 为所有 API 服务类编写单元测试
- 为认证工具类编写测试
- 为关键组件编写组件测试

### 集成测试
- 测试认证流程集成
- 测试 API 调用集成
- 测试路由权限集成

### E2E 测试
- 用户登录流程测试
- 主要功能模块的端到端测试
- 权限控制测试

### 测试配置
更新测试配置文件，添加必要的测试工具和模拟数据。

## [Implementation Order]
详细说明实施步骤的逻辑顺序。

### 第一阶段：基础设施搭建（1-2周）
1. **环境配置**
   - 更新 DFApp.Vue 项目依赖
   - 配置开发和生产环境变量
   - 设置 Vite 代理配置

2. **认证系统集成**
   - 实现 OpenIddict 客户端集成
   - 创建认证工具类和状态管理
   - 实现登录/登出功能

3. **HTTP 客户端封装**
   - 创建 axios 封装类
   - 实现请求/响应拦截器
   - 添加错误处理机制

### 第二阶段：API 层开发（2-3周）
4. **API 服务层创建**
   - 创建各模块的 API 服务类
   - 实现标准的 CRUD 操作
   - 添加类型定义和接口

5. **路由和权限系统**
   - 重新设计前端路由结构
   - 实现基于角色的权限控制
   - 添加路由守卫

### 第三阶段：组件迁移（3-4周）
6. **现有 VueApp 组件迁移**
   - 迁移 Expenditure 分析组件
   - 迁移 LogSink 组件
   - 迁移 Media 相关组件
   - 迁移 TG 相关组件

7. **Razor Pages 转换**
   - 转换首页 (Index.cshtml)
   - 转换 Aria2 管理页面
   - 转换配置管理页面
   - 转换动态IP页面

8. **复杂模块迁移**
   - 迁移 Bookkeeping 模块（分类和支出管理）
   - 迁移 Lottery 模块（彩票管理和统计）
   - 迁移 FileUploadDownload 模块
   - 迁移 LogViewer 模块

### 第四阶段：集成和优化（1-2周）
9. **功能集成测试**
   - 端到端功能测试
   - 性能优化
   - 用户体验改进

10. **部署准备**
    - 生产环境配置
    - 构建优化
    - 部署脚本准备

### 第五阶段：上线和清理（1周）
11. **生产部署**
    - 独立前端应用部署
    - 数据库迁移（如需要）
    - 监控和日志配置

12. **代码清理**
    - 删除旧的 VueApp 目录
    - 删除不需要的 Razor Pages
    - 更新文档和README
