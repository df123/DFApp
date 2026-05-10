# RSS镜像站点功能前端文档

## 目录

- [功能概述](#功能概述)
- [技术栈](#技术栈)
- [项目结构](#项目结构)
- [文件组织](#文件组织)
- [API封装](#api封装)
- [类型定义](#类型定义)
- [页面组件](#页面组件)
- [路由配置](#路由配置)
- [组件详解](#组件详解)
- [状态管理](#状态管理)
- [样式说明](#样式说明)
- [使用流程](#使用流程)
- [故障排查](#故障排查)

---

## 功能概述

RSS镜像站点前端提供了三个主要页面，用于管理RSS源、查看RSS镜像条目和分词统计。

### 页面列表

1. **RSS源管理** (`/download-subscription/rss-sources`)
   - 管理RSS源配置
   - 新增/编辑/删除RSS源
   - 手动触发抓取
   - 查看抓取状态和错误信息

2. **RSS镜像条目** (`/download-subscription/rss-mirror-items`)
   - 查看所有RSS镜像条目
   - 多条件筛选和搜索
   - 下载到Aria2
   - 批量删除

3. **分词统计** (`/download-subscription/rss-word-segments`)
   - 全部分词统计查看（支持分页）
   - 分词详情列表查看
   - 按分词精确筛选
   - 直接下载到Aria2

### 核心特性

- ✅ 完全响应式设计，适配各种屏幕尺寸
- ✅ Element Plus UI组件，一致的用户体验
- ✅ TypeScript类型安全
- ✅ 分页查询，性能优化
- ✅ 实时状态更新
- ✅ 优雅的错误处理
- ✅ 直观的操作反馈

---

## 技术栈

### 核心框架

```json
{
  "vue": "^3.3.0",
  "typescript": "^5.0.0",
  "element-plus": "^2.4.0",
  "vite": "^5.0.0",
  "vue-router": "^4.2.0"
}
```

### 开发工具

- **构建工具**: Vite
- **语言**: TypeScript
- **UI框架**: Element Plus
- **路由**: Vue Router
- **HTTP客户端**: 自定义封装（基于axios）
- **状态管理**: Composition API (reactive/ref)

### 代码风格

- **组合式API**: 使用 `<script setup>` 语法
- **类型系统**: 完整的TypeScript类型定义
- **组件通信**: Props / Emits
- **样式**: Scoped CSS

---

## 项目结构

```
DFApp.Vue/
├── src/
│   ├── api/
│   │   ├── rssSource.ts          # RSS源API封装
│   │   ├── rssMirror.ts          # RSS镜像条目API封装
│   │   └── rssWordSegment.ts     # 分词统计API封装
│   │
│   ├── types/
│   │   └── business.ts           # 业务类型定义（包含RSS相关类型）
│   │
│   ├── views/
│   │   ├── rss-mirror/
│   │   │   ├── sources/
│   │   │   │   └── index.vue     # RSS源管理页面
│   │   │   └── items/
│   │   │       └── index.vue     # RSS镜像条目页面
│   │   └── rss/
│   │       └── word-segments.vue # 分词统计页面
│   │
│   └── router/
│       └── modules/
│           └── download-subscription.ts  # 路由配置
│
├── docs/
│   └── rss-mirror-feature.md     # 本文档
│
└── package.json
```

---

## 文件组织

### 新增文件列表

#### API层

1. **`/src/api/rssSource.ts`**
   - RSS源API封装
   - 包含CRUD操作和手动触发抓取

2. **`/src/api/rssMirror.ts`**
   - RSS镜像条目API封装
   - 包含查询、删除、统计、下载等操作

3. **`/src/api/rssWordSegment.ts`**
   - 分词统计API封装
   - 包含分词列表查询、统计查询等操作

#### 类型层

4. **`/src/types/business.ts`** (扩展)
   - 添加了RSS相关的TypeScript类型定义

#### 页面组件

5. **`/src/views/rss-mirror/sources/index.vue`**
   - RSS源管理页面组件

6. **`/src/views/rss-mirror/items/index.vue`**
   - RSS镜像条目页面组件

7. **`/src/views/rss/word-segments.vue`**
   - 分词统计页面组件

#### 路由配置

8. **`/src/router/modules/download-subscription.ts`** (修改)
   - 添加了RSS镜像相关路由

---

## API封装

### 1. RSS源API (`/src/api/rssSource.ts`)

```typescript
import { http } from "@/utils/http";
import type { PagedRequestDto, PagedResultDto } from "@/types/api";
import type { RssSourceDto, CreateUpdateRssSourceDto } from "@/types/business";

class RssSourceApi {
  private baseUrl = "/api/app/rss-source";

  /**
   * 获取RSS源列表
   */
  async getList(
    params?: PagedRequestDto
  ): Promise<PagedResultDto<RssSourceDto>> {
    return http.get(this.baseUrl, { params });
  }

  /**
   * 获取RSS源详情
   */
  async get(id: number): Promise<RssSourceDto> {
    return http.get(`${this.baseUrl}/${id}`);
  }

  /**
   * 创建RSS源
   */
  async create(input: CreateUpdateRssSourceDto): Promise<RssSourceDto> {
    return http.post(this.baseUrl, { data: input });
  }

  /**
   * 更新RSS源
   */
  async update(
    id: number,
    input: CreateUpdateRssSourceDto
  ): Promise<RssSourceDto> {
    return http.put(`${this.baseUrl}/${id}`, { data: input });
  }

  /**
   * 删除RSS源
   */
  async delete(id: number): Promise<void> {
    return http.request("delete", `${this.baseUrl}/${id}`);
  }

  /**
   * 手动触发RSS源抓取
   */
  async triggerFetch(id: number): Promise<void> {
    return http.post(`${this.baseUrl}/${id}/trigger-fetch`);
  }
}

export const rssSourceApi = new RssSourceApi();
export function useRssSourceApi() {
  return rssSourceApi;
}
export default rssSourceApi;
```

**使用示例**:

```typescript
import { rssSourceApi } from "@/api/rssSource";

// 获取列表
const result = await rssSourceApi.getList({
  skipCount: 0,
  maxResultCount: 20
});

// 创建RSS源
const newSource = await rssSourceApi.create({
  name: "Sukebei Nyaa",
  url: "https://sukebei.nyaa.si/?page=rss",
  isEnabled: true,
  fetchIntervalMinutes: 5,
  maxItems: 50
});

// 触发抓取
await rssSourceApi.triggerFetch(sourceId);
```

### 2. RSS镜像条目API (`/src/api/rssMirror.ts`)

```typescript
import { http } from "@/utils/http";
import type { PagedRequestDto, PagedResultDto } from "@/types/api";
import type {
  RssMirrorItemDto,
  GetRssMirrorItemsRequestDto,
  WordSegmentStatisticsDto
} from "@/types/business";

class RssMirrorApi {
  private baseUrl = "/api/app/rss-mirror-item";

  /**
   * 获取RSS镜像条目列表
   */
  async getList(
    input: GetRssMirrorItemsRequestDto
  ): Promise<PagedResultDto<RssMirrorItemDto>> {
    return http.get(this.baseUrl, { params: input });
  }

  /**
   * 获取RSS镜像条目详情
   */
  async get(id: number): Promise<RssMirrorItemDto> {
    return http.get(`${this.baseUrl}/${id}`);
  }

  /**
   * 删除RSS镜像条目
   */
  async delete(id: number): Promise<void> {
    return http.request("delete", `${this.baseUrl}/${id}`);
  }

  /**
   * 批量删除RSS镜像条目
   */
  async deleteMany(ids: number[]): Promise<void> {
    return http.request("delete", `${this.baseUrl}/many`, { data: ids });
  }

  /**
   * 获取分词统计
   */
  async getWordSegmentStatistics(
    rssSourceId?: number,
    languageType?: number,
    top: number = 100
  ): Promise<WordSegmentStatisticsDto[]> {
    return http.get(`${this.baseUrl}/word-segment-statistics`, {
      params: { rssSourceId, languageType, top }
    });
  }

  /**
   * 根据分词查询RSS镜像条目
   */
  async getByWordToken(
    wordToken: string,
    params?: PagedRequestDto
  ): Promise<PagedResultDto<RssMirrorItemDto>> {
    return http.get(`${this.baseUrl}/by-word-token`, {
      params: { wordToken, ...params }
    });
  }

  /**
   * 清空所有RSS镜像条目
   */
  async clearAll(): Promise<void> {
    return http.post(`${this.baseUrl}/clear-all`);
  }

  /**
   * 下载到Aria2
   */
  async downloadToAria2(
    id: number,
    videoOnly: boolean = false,
    enableKeywordFilter: boolean = false
  ): Promise<string> {
    return http.post(`${this.baseUrl}/${id}/download-to-aria2`, {
      params: { videoOnly, enableKeywordFilter }
    });
  }
}

export const rssMirrorApi = new RssMirrorApi();
export function useRssMirrorApi() {
  return rssMirrorApi;
}
export default rssMirrorApi;
```

**使用示例**:

```typescript
import { rssMirrorApi } from "@/api/rssMirror";

// 获取列表
const result = await rssMirrorApi.getList({
  skipCount: 0,
  maxResultCount: 20,
  rssSourceId: 1,
  filter: "keyword"
});

// 获取分词统计
const statistics = await rssMirrorApi.getWordSegmentStatistics(
  undefined, // rssSourceId (可选)
  1, // languageType (0=中文, 1=英文, 2=日文)
  100 // top
);

// 下载到Aria2
const gid = await rssMirrorApi.downloadToAria2(
  itemId,
  true, // videoOnly
  true // enableKeywordFilter
);
```

### HTTP客户端说明

**`/src/utils/http`**:

- 封装了axios
- 自动处理请求/响应拦截
- 统一错误处理
- 自动添加认证token
- 支持请求取消

**请求方法**:

```typescript
// GET请求
http.get(url, { params });

// POST请求
http.post(url, { data });

// PUT请求
http.put(url, { data });

// DELETE请求
http.request("delete", url);

// 通用请求
http.request(method, url, config);
```

---

## 类型定义

### RSS相关类型 (`/src/types/business.ts`)

#### 1. RSS源DTO

```typescript
export interface RssSourceDto {
  id: number; // RSS源ID
  name: string; // RSS源名称
  url: string; // RSS Feed URL
  proxyUrl?: string; // 代理地址
  proxyUsername?: string; // 代理用户名
  proxyPassword?: string; // 代理密码
  isEnabled: boolean; // 是否启用
  fetchIntervalMinutes: number; // 抓取间隔（分钟）
  maxItems: number; // 最大条目数
  query?: string; // 搜索关键词
  lastFetchTime?: string; // 最后抓取时间（ISO 8601）
  fetchStatus: number; // 抓取状态（0=正常, 1=抓取中, 2=失败）
  errorMessage?: string; // 错误信息
  remark?: string; // 备注
  creationTime: string; // 创建时间（ISO 8601）
}
```

#### 2. 创建/更新RSS源DTO

```typescript
export interface CreateUpdateRssSourceDto {
  name: string; // RSS源名称（必填）
  url: string; // RSS Feed URL（必填）
  proxyUrl?: string; // 代理地址（可选）
  proxyUsername?: string; // 代理用户名（可选）
  proxyPassword?: string; // 代理密码（可选）
  isEnabled: boolean; // 是否启用（必填）
  fetchIntervalMinutes: number; // 抓取间隔（必填）
  maxItems: number; // 最大条目数（必填）
  query?: string; // 搜索关键词（可选）
  remark?: string; // 备注（可选）
}
```

#### 3. RSS镜像条目DTO

```typescript
export interface RssMirrorItemDto {
  id: number; // 条目ID
  rssSourceId: number; // RSS源ID
  rssSourceName?: string; // RSS源名称（关联查询）
  title: string; // 标题
  link: string; // 链接
  description?: string; // 描述
  author?: string; // 作者
  category?: string; // 分类
  publishDate?: string; // 发布时间（ISO 8601）
  seeders?: number; // 做种人数（BT专用）
  leechers?: number; // 下载人数（BT专用）
  downloads?: number; // 完成下载次数（BT专用）
  extensions?: string; // 扩展信息（JSON）
  isDownloaded: boolean; // 是否已下载
  downloadTime?: string; // 下载时间（ISO 8601）
  creationTime: string; // 抓取时间（ISO 8601）
  wordSegments?: RssWordSegmentDto[]; // 分词列表
}
```

#### 4. 分词DTO

```typescript
export interface RssWordSegmentDto {
  id?: number; // 分词记录ID
  rssMirrorItemId: number; // 关联的条目ID
  word: string; // 分词内容
  languageType: number; // 语言类型（0=中文, 1=英文, 2=日文）
  count: number; // 出现次数
  partOfSpeech?: string; // 词性
  creationTime: string; // 创建时间（ISO 8601）
}
```

#### 5. 查询请求DTO

```typescript
export interface GetRssMirrorItemsRequestDto extends PagedRequestDto {
  rssSourceId?: number; // RSS源ID筛选
  filter?: string; // 关键词搜索（标题或描述）
  startTime?: string; // 开始时间（ISO 8601）
  endTime?: string; // 结束时间（ISO 8601）
  isDownloaded?: boolean; // 下载状态筛选
  wordToken?: string; // 分词筛选
}
```

**继承的基础类型**:

```typescript
export interface PagedRequestDto {
  skipCount: number; // 跳过条数
  maxResultCount: number; // 最大返回条数
  sorting?: string; // 排序
}

export interface PagedResultDto<T> {
  totalCount: number; // 总条数
  items: T[]; // 数据列表
}
```

#### 6. 分词统计DTO

```typescript
export interface WordSegmentStatisticsDto {
  word: string; // 分词
  totalCount: number; // 总出现次数
  itemCount: number; // 涉及条目数
  languageType: number; // 语言类型
}
```

---

## 页面组件

### 1. RSS源管理页面

**路径**: `/src/views/rss-mirror/sources/index.vue`

#### 功能特性

- ✅ RSS源列表展示
- ✅ 搜索和过滤（关键词、状态）
- ✅ 分页查询
- ✅ 新增RSS源
- ✅ 编辑RSS源
- ✅ 删除RSS源（带确认）
- ✅ 手动触发抓取
- ✅ 实时状态显示

#### 组件结构

**模板部分**:

```vue
<template>
  <div class="rss-source-container">
    <!-- 页面卡片 -->
    <el-card>
      <!-- 卡片头部：标题 + 新增按钮 -->
      <template #header>
        <div class="card-header">
          <span class="card-title">RSS源管理</span>
          <el-button type="primary" @click="handleAdd">新增RSS源</el-button>
        </div>
      </template>

      <!-- 搜索表单 -->
      <el-form :inline="true" :model="searchForm">
        <el-form-item label="关键词">
          <el-input v-model="searchForm.filter" />
        </el-form-item>
        <el-form-item label="状态">
          <el-select v-model="searchForm.isEnabled">
            <el-option label="启用" :value="true" />
            <el-option label="禁用" :value="false" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">搜索</el-button>
          <el-button @click="handleReset">重置</el-button>
        </el-form-item>
      </el-form>

      <!-- 数据表格 -->
      <el-table :data="tableData" v-loading="loading">
        <el-table-column prop="name" label="名称" />
        <el-table-column prop="url" label="URL" show-overflow-tooltip />
        <el-table-column label="状态">
          <template #default="{ row }">
            <el-tag :type="row.isEnabled ? 'success' : 'info'">
              {{ row.isEnabled ? "启用" : "禁用" }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="fetchIntervalMinutes" label="抓取间隔(分钟)" />
        <el-table-column prop="maxItems" label="最大条目数" />
        <el-table-column prop="lastFetchTime" label="最后抓取时间">
          <template #default="{ row }">
            {{ row.lastFetchTime ? formatDate(row.lastFetchTime) : "-" }}
          </template>
        </el-table-column>
        <el-table-column label="抓取状态">
          <template #default="{ row }">
            <el-tag v-if="row.fetchStatus === 0" type="success">正常</el-tag>
            <el-tag v-else-if="row.fetchStatus === 1" type="warning"
              >抓取中</el-tag
            >
            <el-tag v-else type="danger">失败</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作">
          <template #default="{ row }">
            <el-button @click="handleTriggerFetch(row)">立即抓取</el-button>
            <el-button @click="handleEdit(row)">编辑</el-button>
            <el-button @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <!-- 分页 -->
      <el-pagination
        v-model:current-page="pagination.page"
        v-model:page-size="pagination.pageSize"
        :total="pagination.total"
        @current-change="fetchData"
        @size-change="fetchData"
      />
    </el-card>

    <!-- 新增/编辑对话框 -->
    <el-dialog
      v-model="dialogVisible"
      :title="isEdit ? '编辑RSS源' : '新增RSS源'"
    >
      <el-form ref="formRef" :model="formData" :rules="formRules">
        <el-form-item label="名称" prop="name">
          <el-input v-model="formData.name" />
        </el-form-item>
        <el-form-item label="URL" prop="url">
          <el-input v-model="formData.url" />
        </el-form-item>
        <!-- 其他表单字段... -->
      </el-form>
    </el-dialog>
  </div>
</template>
```

**脚本部分**:

```typescript
<script setup lang="ts">
import { ref, reactive, onMounted } from "vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { rssSourceApi } from "@/api/rssSource";
import type { RssSourceDto, CreateUpdateRssSourceDto } from "@/types/business";

// 搜索表单
const searchForm = reactive({
  filter: "",
  isEnabled: undefined as boolean | undefined
});

// 分页
const pagination = reactive({
  page: 1,
  pageSize: 20,
  total: 0
});

// 表格数据
const loading = ref(false);
const tableData = ref<RssSourceDto[]>([]);

// 对话框状态
const dialogVisible = ref(false);
const isEdit = ref(false);
const formRef = ref();
const formData = reactive<CreateUpdateRssSourceDto>({
  name: "",
  url: "",
  isEnabled: true,
  fetchIntervalMinutes: 5,
  maxItems: 50,
  proxyUrl: "",
  proxyUsername: "",
  proxyPassword: "",
  query: "",
  remark: ""
});

// 表单验证规则
const formRules = {
  name: [{ required: true, message: "请输入名称", trigger: "blur" }],
  url: [
    { required: true, message: "请输入URL", trigger: "blur" },
    { type: "url", message: "请输入有效的URL", trigger: "blur" }
  ],
  fetchIntervalMinutes: [
    { required: true, message: "请输入抓取间隔", trigger: "blur" }
  ],
  maxItems: [
    { required: true, message: "请输入最大条目数", trigger: "blur" }
  ]
};

let currentEditId: number | null = null;

// 获取数据
const fetchData = async () => {
  loading.value = true;
  try {
    const result = await rssSourceApi.getList({
      skipCount: (pagination.page - 1) * pagination.pageSize,
      maxResultCount: pagination.pageSize,
      filter: searchForm.filter || undefined,
      sorting: "CreationTime desc"
    });
    tableData.value = result.items || [];
    pagination.total = result.totalCount || 0;
  } catch (error) {
    console.error("获取RSS源列表失败:", error);
    ElMessage.error("获取RSS源列表失败");
  } finally {
    loading.value = false;
  }
};

// 搜索
const handleSearch = () => {
  pagination.page = 1;
  fetchData();
};

// 重置
const handleReset = () => {
  searchForm.filter = "";
  searchForm.isEnabled = undefined;
  handleSearch();
};

// 新增
const handleAdd = () => {
  isEdit.value = false;
  currentEditId = null;
  Object.assign(formData, {
    name: "",
    url: "",
    isEnabled: true,
    fetchIntervalMinutes: 5,
    maxItems: 50,
    proxyUrl: "",
    proxyUsername: "",
    proxyPassword: "",
    query: "",
    remark: ""
  });
  dialogVisible.value = true;
};

// 编辑
const handleEdit = (row: RssSourceDto) => {
  isEdit.value = true;
  currentEditId = row.id;
  Object.assign(formData, {
    name: row.name,
    url: row.url,
    isEnabled: row.isEnabled,
    fetchIntervalMinutes: row.fetchIntervalMinutes,
    maxItems: row.maxItems,
    proxyUrl: row.proxyUrl || "",
    proxyUsername: row.proxyUsername || "",
    proxyPassword: row.proxyPassword || "",
    query: row.query || "",
    remark: row.remark || ""
  });
  dialogVisible.value = true;
};

// 提交
const handleSubmit = async () => {
  if (!formRef.value) return;

  const valid = await formRef.value.validate();
  if (!valid) return;

  try {
    if (isEdit.value && currentEditId) {
      await rssSourceApi.update(currentEditId, formData);
      ElMessage.success("更新成功");
    } else {
      await rssSourceApi.create(formData);
      ElMessage.success("创建成功");
    }
    dialogVisible.value = false;
    fetchData();
  } catch (error: any) {
    console.error("提交失败:", error);
    ElMessage.error(error.message || "提交失败");
  }
};

// 删除
const handleDelete = async (row: RssSourceDto) => {
  try {
    await ElMessageBox.confirm(
      `确定要删除RSS源 "${row.name}" 吗？`,
      "提示",
      {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning"
      }
    );

    await rssSourceApi.delete(row.id);
    ElMessage.success("删除成功");
    fetchData();
  } catch (error) {
    if (error !== "cancel") {
      console.error("删除失败:", error);
      ElMessage.error("删除失败");
    }
  }
};

// 立即抓取
const handleTriggerFetch = async (row: RssSourceDto) => {
  if (!row.isEnabled) {
    ElMessage.warning("该RSS源未启用");
    return;
  }

  try {
    await rssSourceApi.triggerFetch(row.id);
    ElMessage.success("已触发抓取任务");
    fetchData();
  } catch (error: any) {
    console.error("触发抓取失败:", error);
    ElMessage.error(error.message || "触发抓取失败");
  }
};

// 格式化日期
const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleString("zh-CN");
};

// 生命周期
onMounted(() => {
  fetchData();
});
</script>
```

**样式部分**:

```vue
<style scoped>
.rss-source-container {
  height: 100%;
  padding: 20px;
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.card-title {
  font-size: 16px;
  font-weight: 600;
  color: #303133;
}

.search-form {
  margin-bottom: 20px;
}
</style>
```

### 2. RSS镜像条目页面

**路径**: `/src/views/rss-mirror/items/index.vue`

#### 功能特性

- ✅ 镜像条目列表展示
- ✅ 多维度筛选（RSS源、关键词、分词、下载状态、时间范围）
- ✅ 分页查询
- ✅ 单个/批量删除
- ✅ 清空所有数据
- ✅ 下载到Aria2（带配置选项）
- ✅ 分词统计展示
- ✅ 按分词快速过滤
- ✅ 分词详情查看
- ✅ 外链打开

#### 关键组件

**搜索表单**:

```vue
<el-form :inline="true" :model="searchForm">
  <el-form-item label="RSS源">
    <el-select v-model="searchForm.rssSourceId" placeholder="全部" clearable>
      <el-option
        v-for="source in rssSources"
        :key="source.id"
        :label="source.name"
        :value="source.id"
      />
    </el-select>
  </el-form-item>

  <el-form-item label="关键词">
    <el-input v-model="searchForm.filter" placeholder="搜索标题或描述" />
  </el-form-item>

  <el-form-item label="分词">
    <el-input v-model="searchForm.wordToken" placeholder="输入分词过滤" />
  </el-form-item>

  <el-form-item label="下载状态">
    <el-select v-model="searchForm.isDownloaded" placeholder="全部" clearable>
      <el-option label="已下载" :value="true" />
      <el-option label="未下载" :value="false" />
    </el-select>
  </el-form-item>

  <el-form-item label="时间范围">
    <el-date-picker
      v-model="dateRange"
      type="datetimerange"
      range-separator="至"
      start-placeholder="开始时间"
      end-placeholder="结束时间"
      value-format="YYYY-MM-DD HH:mm:ss"
      @change="handleDateChange"
    />
  </el-form-item>

  <el-form-item>
    <el-button type="primary" @click="handleSearch">搜索</el-button>
    <el-button @click="handleReset">重置</el-button>
  </el-form-item>
</el-form>
```

**数据表格**:

```vue
<el-table
  v-loading="loading"
  :data="tableData"
  @selection-change="handleSelectionChange"
>
  <el-table-column type="selection" width="55" />
  <el-table-column prop="title" label="标题" show-overflow-tooltip />
  <el-table-column prop="rssSourceName" label="RSS源" />
  <el-table-column prop="link" label="链接">
    <template #default="{ row }">
      <el-button link type="primary" @click="openLink(row.link)">
        打开
      </el-button>
    </template>
  </el-table-column>
  <el-table-column prop="publishDate" label="发布时间">
    <template #default="{ row }">
      {{ row.publishDate ? formatDate(row.publishDate) : '-' }}
    </template>
  </el-table-column>
  <el-table-column prop="seeders" label="做种">
    <template #default="{ row }">
      <el-tag v-if="row.seeders !== null" type="success">
        {{ row.seeders }}
      </el-tag>
      <span v-else>-</span>
    </template>
  </el-table-column>
  <el-table-column prop="leechers" label="下载">
    <template #default="{ row }">
      <el-tag v-if="row.leechers !== null" type="warning">
        {{ row.leechers }}
      </el-tag>
      <span v-else>-</span>
    </template>
  </el-table-column>
  <el-table-column label="下载状态">
    <template #default="{ row }">
      <el-tag :type="row.isDownloaded ? 'success' : 'info'">
        {{ row.isDownloaded ? '已下载' : '未下载' }}
      </el-tag>
    </template>
  </el-table-column>
  <el-table-column label="分词">
    <template #default="{ row }">
      <el-button
        v-if="row.wordSegments && row.wordSegments.length > 0"
        link
        type="primary"
        @click="showWordSegments(row)"
      >
        查看({{ row.wordSegments.length }})
      </el-button>
      <span v-else>-</span>
    </template>
  </el-table-column>
  <el-table-column label="操作">
    <template #default="{ row }">
      <el-button
        size="small"
        type="primary"
        link
        :disabled="row.isDownloaded"
        @click="handleDownload(row)"
      >
        下载
      </el-button>
      <el-button
        size="small"
        type="danger"
        link
        @click="handleDelete(row)"
      >
        删除
      </el-button>
    </template>
  </el-table-column>
</el-table>
```

**分词统计对话框**:

```vue
<el-dialog v-model="showStatistics" title="分词统计" width="80%">
  <div class="statistics-container">
    <!-- 筛选表单 -->
    <el-form :inline="true">
      <el-form-item label="语言类型">
        <el-select v-model="statisticsForm.languageType" placeholder="全部" clearable>
          <el-option label="中文" :value="0" />
          <el-option label="英文" :value="1" />
          <el-option label="日文" :value="2" />
        </el-select>
      </el-form-item>
      <el-form-item label="Top">
        <el-input-number v-model="statisticsForm.top" :min="10" :max="500" :step="10" />
      </el-form-item>
      <el-form-item>
        <el-button type="primary" @click="fetchStatistics">查询</el-button>
      </el-form-item>
    </el-form>

    <!-- 统计表格 -->
    <el-table :data="statisticsData" v-loading="statisticsLoading">
      <el-table-column prop="word" label="分词" />
      <el-table-column prop="languageType" label="语言类型">
        <template #default="{ row }">
          <el-tag v-if="row.languageType === 0" type="success">中文</el-tag>
          <el-tag v-else-if="row.languageType === 1" type="primary">英文</el-tag>
          <el-tag v-else-if="row.languageType === 2" type="warning">日文</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="totalCount" label="总出现次数" />
      <el-table-column prop="itemCount" label="涉及条目数" />
      <el-table-column label="操作">
        <template #default="{ row }">
          <el-button type="primary" link @click="filterByWord(row.word)">
            查看条目
          </el-button>
        </template>
      </el-table-column>
    </el-table>
  </div>
</el-dialog>
```

**下载选项对话框**:

```vue
<el-dialog v-model="showDownloadDialog" title="下载选项" width="500px">
  <el-form label-width="140px">
    <el-form-item label="仅下载视频">
      <el-switch v-model="downloadOptions.videoOnly" />
      <div class="form-tip">仅适用于.torrent文件</div>
    </el-form-item>
    <el-form-item label="启用关键词过滤">
      <el-switch v-model="downloadOptions.enableKeywordFilter" />
      <div class="form-tip">根据关键词过滤规则过滤文件</div>
    </el-form-item>
  </el-form>
  <template #footer>
    <el-button @click="showDownloadDialog = false">取消</el-button>
    <el-button type="primary" @click="confirmDownload">确定</el-button>
  </template>
</el-dialog>
```

#### 关键逻辑

**日期范围处理**:

```typescript
const dateRange = ref<[string, string]>([]);

const handleDateChange = (value: [string, string] | null) => {
  if (value && value.length === 2) {
    searchForm.startTime = value[0];
    searchForm.endTime = value[1];
  } else {
    searchForm.startTime = "";
    searchForm.endTime = "";
  }
  handleSearch();
};
```

**批量选择**:

```typescript
const selectedRows = ref<RssMirrorItemDto[]>([]);

const handleSelectionChange = (rows: RssMirrorItemDto[]) => {
  selectedRows.value = rows;
};

const handleBatchDelete = async () => {
  try {
    await ElMessageBox.confirm(
      `确定要删除选中的 ${selectedRows.value.length} 项吗？`,
      "提示",
      {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning"
      }
    );

    const ids = selectedRows.value.map(row => row.id);
    await rssMirrorApi.deleteMany(ids);
    ElMessage.success("批量删除成功");
    fetchData();
  } catch (error) {
    if (error !== "cancel") {
      console.error("批量删除失败:", error);
      ElMessage.error("批量删除失败");
    }
  }
};
```

**按分词过滤**:

```typescript
const filterByWord = (word: string) => {
  searchForm.wordToken = word;
  showStatistics.value = false;
  showWordSegmentsDialog.value = false;
  handleSearch();
};
```

---

## 路由配置

### 路由文件

**路径**: `/src/router/modules/download-subscription.ts`

```typescript
export default {
  path: "/download-subscription",
  name: "DownloadSubscription",
  redirect: "/download-subscription/aria2",
  meta: {
    title: "下载与订阅",
    icon: "ep:download",
    rank: 4
  },
  children: [
    {
      path: "/download-subscription/aria2",
      name: "Aria2Subscription",
      component: () => import("@/views/aria2/index.vue"),
      meta: {
        title: "Aria2管理"
      }
    },
    {
      path: "/download-subscription/download-manage",
      name: "DownloadManage",
      component: () => import("@/views/aria2/manage.vue"),
      meta: {
        title: "下载管理"
      }
    },
    {
      path: "/download-subscription/rss",
      name: "Rss",
      component: () => import("@/views/rss/index.vue"),
      meta: {
        title: "RSS阅读器"
      }
    },
    {
      path: "/download-subscription/rss-sources",
      name: "RssSources",
      component: () => import("@/views/rss-mirror/sources/index.vue"),
      meta: {
        title: "RSS源管理"
      }
    },
    {
      path: "/download-subscription/rss-mirror-items",
      name: "RssMirrorItems",
      component: () => import("@/views/rss-mirror/items/index.vue"),
      meta: {
        title: "RSS镜像条目"
      }
    },
    {
      path: "/download-subscription/rss-word-segments",
      name: "RssWordSegments",
      component: () => import("@/views/rss/word-segments.vue"),
      meta: {
        title: "分词统计"
      }
    },
    {
      path: "/download-subscription/filterKeyword",
      name: "FilterKeyword",
      component: () => import("@/views/filterKeyword/index.vue"),
      meta: {
        title: "关键词过滤管理"
      }
    }
  ]
} satisfies RouteConfigsTable;
```

### 路由说明

**RSS镜像相关路由**:

1. `/download-subscription/rss-sources` - RSS源管理
2. `/download-subscription/rss-mirror-items` - RSS镜像条目
3. `/download-subscription/rss-word-segments` - 分词统计

**路由元信息**:

- `title`: 显示在菜单和页面标题
- `icon`: 菜单图标（使用Element Plus图标）
- `rank`: 菜单排序权重

**菜单显示**:
路由会自动注册到侧边栏菜单，在"下载与订阅"分组下显示。

---

## 组件详解

### 1. Element Plus组件使用

#### 表单组件

```vue
<!-- 输入框 -->
<el-input v-model="value" placeholder="请输入" clearable />

<!-- 数字输入框 -->
<el-input-number v-model="value" :min="1" :max="100" />

<!-- 选择器 -->
<el-select v-model="value" placeholder="请选择" clearable>
  <el-option label="选项1" :value="1" />
  <el-option label="选项2" :value="2" />
</el-select>

<!-- 日期时间选择器 -->
<el-date-picker
  v-model="value"
  type="datetimerange"
  value-format="YYYY-MM-DD HH:mm:ss"
/>

<!-- 开关 -->
<el-switch v-model="value" />
```

#### 表格组件

```vue
<el-table :data="tableData" v-loading="loading" stripe>
  <!-- 选择列 -->
  <el-table-column type="selection" width="55" />

  <!-- 普通列 -->
  <el-table-column prop="name" label="名称" />

  <!-- 自定义列 -->
  <el-table-column label="状态">
    <template #default="{ row }">
      <el-tag :type="row.status ? 'success' : 'danger'">
        {{ row.status ? '启用' : '禁用' }}
      </el-tag>
    </template>
  </el-table-column>

  <!-- 操作列 -->
  <el-table-column label="操作" fixed="right">
    <template #default="{ row }">
      <el-button size="small" link @click="handleEdit(row)">编辑</el-button>
      <el-button size="small" link type="danger" @click="handleDelete(row)">删除</el-button>
    </template>
  </el-table-column>
</el-table>
```

#### 对话框组件

```vue
<el-dialog
  v-model="visible"
  title="标题"
  width="600px"
  :close-on-click-modal="false"
>
  <!-- 内容 -->
  <div>对话框内容</div>

  <!-- 底部按钮 -->
  <template #footer>
    <el-button @click="visible = false">取消</el-button>
    <el-button type="primary" @click="handleSubmit">确定</el-button>
  </template>
</el-dialog>
```

#### 消息提示

```typescript
import { ElMessage, ElMessageBox } from "element-plus";

// 成功消息
ElMessage.success("操作成功");

// 错误消息
ElMessage.error("操作失败");

// 警告消息
ElMessage.warning("请注意");

// 确认对话框
try {
  await ElMessageBox.confirm("确定要删除吗？", "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning"
  });
  // 用户点击确定
} catch (error) {
  // 用户点击取消
}
```

### 2. 组合式API特性

#### reactive vs ref

**reactive**:

```typescript
// 用于对象
const searchForm = reactive({
  filter: "",
  isEnabled: undefined as boolean | undefined
});

// 访问时不需要.value
searchForm.filter = "new value";
```

**ref**:

```typescript
// 用于基本类型或需要整体替换的对象
const loading = ref(false);
const tableData = ref<RssSourceDto[]>([]);

// 访问时需要.value
loading.value = true;
tableData.value = [];
```

#### 计算属性

```typescript
import { computed } from "vue";

const pageCount = computed(() => {
  return Math.ceil(pagination.total / pagination.pageSize);
});
```

#### 侦听器

```typescript
import { watch } from "vue";

watch(
  () => searchForm.filter,
  (newVal, oldVal) => {
    console.log("filter changed:", oldVal, "->", newVal);
  }
);
```

#### 生命周期

```typescript
import { onMounted, onUnmounted } from "vue";

onMounted(() => {
  // 组件挂载后执行
  fetchData();
});

onUnmounted(() => {
  // 组件卸载前执行
  // 清理工作
});
```

### 3. 类型系统

#### 接口定义

```typescript
interface RssSourceDto {
  id: number;
  name: string;
  url: string;
  // ...
}
```

#### 类型注解

```typescript
// 变量类型
const tableData: RssSourceDto[] = [];

// 函数返回类型
const fetchData = async (): Promise<void> => {
  // ...
};

// 函数参数类型
const handleEdit = (row: RssSourceDto) => {
  // ...
};
```

#### 泛型

```typescript
// API返回类型
const result: PagedResultDto<RssSourceDto> = await rssSourceApi.getList(params);

// ref泛型
const tableData = ref<RssSourceDto[]>([]);
```

---

## 状态管理

### 组件内状态

```typescript
// 搜索表单状态
const searchForm = reactive({
  filter: "",
  isEnabled: undefined as boolean | undefined
});

// 分页状态
const pagination = reactive({
  page: 1,
  pageSize: 20,
  total: 0
});

// 加载状态
const loading = ref(false);

// 表格数据
const tableData = ref<RssSourceDto[]>([]);

// 对话框状态
const dialogVisible = ref(false);
const isEdit = ref(false);
```

### 跨组件通信

#### 父子组件通信

```vue
<!-- 父组件 -->
<template>
  <ChildComponent :data="parentData" @update="handleUpdate" />
</template>

<script setup lang="ts">
import ChildComponent from "./ChildComponent.vue";

const parentData = ref({ name: "test" });

const handleUpdate = (newData: any) => {
  console.log("收到子组件更新:", newData);
};
</script>

<!-- 子组件 -->
<template>
  <div @click="emitUpdate">{{ data.name }}</div>
</template>

<script setup lang="ts">
import { defineProps, defineEmits } from "vue";

const props = defineProps<{
  data: { name: string };
}>();

const emit = defineEmits<{
  update: [data: any];
}>();

const emitUpdate = () => {
  emit("update", { name: "updated" });
};
</script>
```

#### Provide/Inject

```typescript
// 祖先组件
import { provide } from "vue";

provide("rssSources", rssSources);

// 后代组件
import { inject } from "vue";

const rssSources = inject<RssSourceDto[]>("rssSources", []);
```

---

## 样式说明

### Scoped CSS

```vue
<style scoped>
/* 只作用于当前组件 */
.container {
  padding: 20px;
}

.title {
  font-size: 16px;
  font-weight: 600;
}
</style>
```

### 全局样式

如果要使用全局样式，不使用`scoped`：

```vue
<style>
/* 全局生效 */
.global-class {
  /* ... */
}
</style>
```

### CSS Modules

```vue
<template>
  <div :class="$style.container">Content</div>
</template>

<style module>
.container {
  padding: 20px;
}
</style>
```

### Element Plus样式覆盖

```vue
<style scoped>
/* 覆盖Element Plus组件样式 */
.el-table {
  font-size: 14px;
}

.el-button {
  margin-left: 10px;
}
</style>
```

### Tailwind CSS

项目支持Tailwind CSS，可以使用工具类：

```vue
<template>
  <div class="flex items-center justify-between p-4">
    <span class="text-lg font-semibold">标题</span>
  </div>
</template>
```

---

## 使用流程

### 1. RSS源管理流程

#### 新增RSS源

1. 导航到"下载与订阅" → "RSS源管理"
2. 点击"新增RSS源"按钮
3. 填写表单：
   - **名称**: 必填，例如"Sukebei Nyaa"
   - **URL**: 必填，RSS Feed地址
   - **是否启用**: 勾选启用
   - **抓取间隔**: 填写分钟数，默认5分钟
   - **最大条目数**: 默认50
   - **搜索关键词**: 可选，用于过滤Feed内容
   - **代理配置**: 如需要代理访问，填写代理信息
   - **备注**: 可选
4. 点击"确定"保存

#### 编辑RSS源

1. 在列表中找到目标RSS源
2. 点击操作列的"编辑"按钮
3. 修改表单内容
4. 点击"确定"保存

#### 删除RSS源

1. 在列表中找到目标RSS源
2. 点击操作列的"删除"按钮
3. 确认删除

#### 手动触发抓取

1. 在列表中找到目标RSS源
2. 确保RSS源已启用
3. 点击操作列的"立即抓取"按钮
4. 系统会提示"已触发抓取任务"
5. 后台任务会在下次调度时优先处理该RSS源

### 2. RSS镜像条目查看流程

#### 查看条目

1. 导航到"下载与订阅" → "RSS镜像条目"
2. 使用筛选条件：
   - 选择RSS源
   - 输入关键词搜索标题
   - 输入分词快速过滤
   - 选择下载状态
   - 设置时间范围
3. 点击"搜索"按钮
4. 查看结果列表

#### 下载内容

1. 在列表中找到目标条目
2. 点击操作列的"下载"按钮
3. 在弹出的对话框中配置：
   - **仅下载视频**: 勾选后下载.torrent文件时只选择视频文件
   - **启用关键词过滤**: 勾选后根据关键词过滤规则过滤文件
4. 点击"确定"添加到Aria2
5. 条目会标记为"已下载"

#### 查看分词统计

1. 点击页面右上角"分词统计"按钮
2. 配置统计条件：
   - 选择语言类型（中文/英文/日文）
   - 设置Top N数量
3. 点击"查询"
4. 查看统计结果
5. 点击分词后的"查看条目"按钮可快速过滤相关条目

#### 批量删除

1. 勾选要删除的条目
2. 点击"批量删除"按钮
3. 确认删除

#### 清空所有

1. 点击页面右上角"清空所有"按钮
2. 确认删除（此操作不可恢复）

### 3. 分词相关操作

#### 查看条目分词

1. 在条目列表中点击"分词"列的"查看"按钮
2. 弹出对话框显示该条目的所有分词
3. 点击分词标签可快速过滤包含该分词的条目

#### 按分词搜索

1. 在搜索表单的"分词"输入框中输入分词
2. 点击"搜索"
3. 系统会返回包含该分词的所有条目

---

## 故障排查

### 1. 页面无法加载

**症状**: 访问页面时白屏或报错

**可能原因**:

- 路由配置错误
- 组件路径错误
- TypeScript类型错误
- API接口未返回数据

**解决方案**:

1. 检查浏览器控制台错误信息
2. 检查路由配置是否正确
3. 检查组件导入路径
4. 运行`npm run typecheck`检查类型错误
5. 检查网络请求是否成功

### 2. 数据不显示

**症状**: 页面加载正常但表格为空

**可能原因**:

- API接口返回数据为空
- 数据格式不匹配
- 搜索条件过于严格
- 后端数据库为空

**解决方案**:

1. 打开浏览器开发者工具 → Network标签
2. 查看API请求是否成功
3. 查看API响应数据格式是否正确
4. 尝试清空搜索条件
5. 检查后端是否有RSS源配置

### 3. 分页不工作

**症状**: 点击分页按钮没有反应

**可能原因**:

- 分页事件未正确绑定
- 分页参数传递错误
- 总数未正确设置

**解决方案**:

```typescript
// 确保正确绑定分页事件
<el-pagination
  v-model:current-page="pagination.page"
  v-model:page-size="pagination.pageSize"
  :total="pagination.total"
  @current-change="fetchData"
  @size-change="fetchData"
/>

// 确保fetchData函数正确处理分页
const fetchData = async () => {
  const result = await api.getList({
    skipCount: (pagination.page - 1) * pagination.pageSize,
    maxResultCount: pagination.pageSize
  });
  pagination.total = result.totalCount;  // 重要：更新总数
};
```

### 4. 日期选择器不工作

**症状**: 选择日期后搜索无结果

**可能原因**:

- 日期格式不匹配
- 时区问题
- 日期未正确传递到API

**解决方案**:

```typescript
// 确保日期格式正确
const handleDateChange = (value: [string, string] | null) => {
  if (value && value.length === 2) {
    searchForm.startTime = value[0]; // "2026-01-14 10:00:00"
    searchForm.endTime = value[1];
    handleSearch();
  }
};

// 检查API请求参数
console.log({
  startTime: searchForm.startTime,
  endTime: searchForm.endTime
});
```

### 5. 批量删除失败

**症状**: 点击批量删除按钮后无反应或报错

**可能原因**:

- 未选中任何项
- 权限不足
- API接口错误

**解决方案**:

```typescript
// 确保检查选中状态
const handleBatchDelete = async () => {
  if (selectedRows.value.length === 0) {
    ElMessage.warning("请先选择要删除的项");
    return;
  }

  // 继续删除操作...
};

// 检查错误信息
try {
  await rssMirrorApi.deleteMany(ids);
  ElMessage.success("批量删除成功");
} catch (error: any) {
  console.error("批量删除失败:", error);
  ElMessage.error(error.message || "批量删除失败");
}
```

### 6. 类型检查错误

**症状**: 运行`npm run typecheck`时报错

**常见错误**:

```
error TS2322: Type 'X' is not assignable to type 'Y'
error TS2531: Object is possibly 'null'
```

**解决方案**:

1. 确保类型定义正确
2. 使用非空断言操作符（`!`）或可选链（`?.`）
3. 添加类型守卫
4. 确保接口和实际数据结构一致

```typescript
// 可能null的对象
const name = row.name?.toLowerCase() || "";

// 非空断言
const id = row.id!;

// 类型守卫
if (row.seeders !== null && row.seeders !== undefined) {
  console.log(row.seeders);
}
```

---

## 性能优化

### 1. 列表优化

#### 虚拟滚动（大数据量）

如果条目数量很大（>1000），考虑使用虚拟滚动：

```vue
<el-table-v2
  :columns="columns"
  :data="tableData"
  :width="700"
  :height="400"
  fixed
/>
```

#### 分页加载

使用合理的分页大小：

```typescript
const pagination = reactive({
  page: 1,
  pageSize: 20, // 不要设置太大，建议10-50
  total: 0
});
```

### 2. 请求优化

#### 防抖和节流

```typescript
import { debounce } from "lodash-es";

// 防抖：搜索输入
const handleSearch = debounce(() => {
  fetchData();
}, 500);

// 节流：滚动加载
const handleScroll = throttle(() => {
  loadMore();
}, 200);
```

#### 取消重复请求

```typescript
let abortController: AbortController | null = null;

const fetchData = async () => {
  // 取消之前的请求
  if (abortController) {
    abortController.abort();
  }

  abortController = new AbortController();

  try {
    const result = await rssSourceApi.getList(
      { skipCount: 0, maxResultCount: 20 },
      { signal: abortController.signal }
    );
    // 处理结果...
  } catch (error) {
    if (error.name !== "AbortError") {
      console.error(error);
    }
  }
};
```

### 3. 渲染优化

#### v-once

```vue
<!-- 只渲染一次 -->
<div v-once>{{ staticContent }}</div>
```

#### v-memo

```vue
<!-- 只有当item.id改变时才重新渲染 -->
<div v-for="item in list" :key="item.id" v-memo="[item.id]">
  {{ item.name }}
</div>
```

#### 计算属性缓存

```typescript
// 使用computed而不是method
const filteredData = computed(() => {
  return tableData.value.filter(item => item.name.includes(searchForm.filter));
});
```

---

## 扩展建议

### 1. 添加图表可视化

使用ECharts或Chart.js展示数据统计：

```vue
<template>
  <div ref="chartRef" style="width: 600px; height: 400px"></div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import * as echarts from "echarts";

const chartRef = ref();

onMounted(() => {
  const chart = echarts.init(chartRef.value);
  chart.setOption({
    title: { text: "分词统计" },
    xAxis: { type: "category", data: ["词1", "词2", "词3"] },
    yAxis: { type: "value" },
    series: [{ type: "bar", data: [100, 80, 60] }]
  });
});
</script>
```

### 2. 添加导出功能

```typescript
// 导出为CSV
const exportToCSV = () => {
  const headers = ["标题", "链接", "发布时间"];
  const rows = tableData.value.map(item => [
    item.title,
    item.link,
    item.publishDate
  ]);

  const csvContent = [
    headers.join(","),
    ...rows.map(row => row.join(","))
  ].join("\n");

  const blob = new Blob([csvContent], { type: "text/csv" });
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = "rss-items.csv";
  a.click();
};
```

### 3. 添加实时更新

使用轮询或WebSocket实时更新数据：

```typescript
import { onMounted, onUnmounted } from "vue";

let intervalId: number | null = null;

onMounted(() => {
  // 每30秒刷新一次
  intervalId = setInterval(() => {
    fetchData();
  }, 30000);
});

onUnmounted(() => {
  if (intervalId) {
    clearInterval(intervalId);
  }
});
```

### 4. 添加拖拽排序

```vue
<template>
  <el-table :data="tableData" row-key="id">
    <el-table-column label="拖拽" width="60">
      <template #default>
        <span class="drag-handle">⋮⋮</span>
      </template>
    </el-table-column>
    <!-- 其他列 -->
  </el-table>
</template>

<script setup lang="ts">
import Sortable from "sortablejs";

onMounted(() => {
  const table = document.querySelector(".el-table__body-wrapper tbody");
  Sortable.create(table as HTMLElement, {
    handle: ".drag-handle",
    onEnd: evt => {
      // 处理排序
      console.log("拖拽排序:", evt.oldIndex, evt.newIndex);
    }
  });
});
</script>
```

---

## 相关资源

### 官方文档

- [Vue 3文档](https://vuejs.org/)
- [TypeScript文档](https://www.typescriptlang.org/)
- [Element Plus文档](https://element-plus.org/)
- [Vite文档](https://vitejs.dev/)
- [Vue Router文档](https://router.vuejs.org/)

### 项目文档

- [后端文档](/home/df/dfapp/DFApp/docs/rss-mirror-feature.md)

### 工具推荐

- **VS Code**: 推荐IDE
- **Vue DevTools**: 浏览器调试工具
- **Postman**: API测试工具

---

## 版本历史

### v1.2.0 (2026-05-10)
**Bug修复与功能重构**

#### 问题描述
1. **分页无效**：RSS镜像条目页面的分页功能无法正常工作，切换页码后数据不更新
2. **下载失败**：点击下载按钮后返回400错误，无法完成下载
3. **超时问题**：下载请求经常超时失败

#### 根本原因
1. **分页参数不匹配**：前端API使用`pageIndex/pageSize`参数名，后端期望`skipCount/maxResultCount`参数名
2. **下载功能未实现**：后端`RssMirrorItemAppService.DownloadToAria2Async`方法为TODO伪代码，未真正注入Aria2Service
3. **下载状态更新时机错误**：`IsDownloaded`状态在请求开始时就标记为true，而非成功后才标记
4. **请求超时过短**：下载相关请求超时设置为10秒，对于大文件下载不够用

#### 修复方案
1. **参数转换**：在`rssMirror.ts`的`getList`方法中添加参数转换逻辑，将`pageIndex/pageSize`转换为`skipCount/maxResultCount`
2. **实现下载功能**：在后端注入真正的Aria2Service，完成下载逻辑实现
3. **修复状态更新**：将`IsDownloaded`状态更新移到下载成功后的回调中
4. **增加超时时间**：下载相关请求超时从10秒改为30秒
5. **优化分页默认值**：默认分页条数从20条改为10条
6. **移除分词统计功能**：根据需求变更，移除前端页面中分词统计相关的UI、API方法和类型定义

#### 修改文件
- `client/src/api/rssMirror.ts` - 参数转换、移除分词统计API、增加下载超时
- `client/src/views/rss-mirror/items/index.vue` - 移除分词统计UI、调整分页默认值
- `src/DFApp.Web/Services/Rss/RssMirrorItemAppService.cs` - 实现下载功能、修复状态更新

#### 验证结果
- ✅ 分页功能正常工作，切换页码数据正确更新
- ✅ 下载功能正常，能够成功添加到Aria2
- ✅ 下载超时问题解决，大文件下载不再超时
- ✅ 下载状态正确显示，只有成功下载后才标记为已下载

### v1.1.0 (2026-05-10)
**文档更新**

- 新增分词统计页面文档
- 更新路由配置（新增 rss-word-segments 路由）
- 更新项目结构（新增 word-segments.vue）
- 移除过时的 ABP Framework 引用

### v1.0.0 (2026-01-14)

**初始版本**

#### 新增功能

- RSS源管理页面
- RSS镜像条目页面
- API封装（rssSource.ts、rssMirror.ts）
- TypeScript类型定义
- 完整的CRUD操作
- 分词统计展示
- Aria2下载集成
- 多条件筛选和搜索
- 批量操作
- 响应式设计

#### 技术栈

- Vue 3.3+
- TypeScript 5.0+
- Element Plus 2.4+
- Vite 5.0+

---

## 作者信息

**开发日期**: 2026-01-14
**版本**: 1.2.0
**框架**: Vue 3 + TypeScript + Element Plus
**AI助手**: Claude (Anthropic)

---

## 许可证

本功能是DFApp项目的一部分，遵循项目整体许可证。
