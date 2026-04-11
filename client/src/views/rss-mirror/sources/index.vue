<template>
  <div class="rss-source-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">RSS源管理</span>
          <el-button type="primary" @click="handleAdd">新增RSS源</el-button>
        </div>
      </template>

      <!-- 搜索表单 -->
      <el-form :inline="true" :model="searchForm" class="search-form">
        <el-form-item label="关键词">
          <el-input
            v-model="searchForm.filter"
            placeholder="搜索名称或URL"
            clearable
            @clear="handleSearch"
          />
        </el-form-item>
        <el-form-item label="状态">
          <el-select
            v-model="searchForm.isEnabled"
            placeholder="全部"
            clearable
            @change="handleSearch"
          >
            <el-option label="启用" :value="true" />
            <el-option label="禁用" :value="false" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">搜索</el-button>
          <el-button @click="handleReset">重置</el-button>
        </el-form-item>
      </el-form>

      <!-- 表格 -->
      <el-table
        v-loading="loading"
        :data="tableData"
        stripe
        style="width: 100%"
      >
        <el-table-column prop="name" label="名称" min-width="150" />
        <el-table-column
          prop="url"
          label="URL"
          min-width="200"
          show-overflow-tooltip
        />
        <el-table-column label="状态" width="80">
          <template #default="{ row }">
            <el-tag :type="row.isEnabled ? 'success' : 'info'">
              {{ row.isEnabled ? "启用" : "禁用" }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column
          prop="fetchIntervalMinutes"
          label="抓取间隔(分钟)"
          width="130"
        />
        <el-table-column prop="maxItems" label="最大条目数" width="100" />
        <el-table-column prop="lastFetchTime" label="最后抓取时间" width="180">
          <template #default="{ row }">
            {{ row.lastFetchTime ? formatDate(row.lastFetchTime) : "-" }}
          </template>
        </el-table-column>
        <el-table-column label="抓取状态" width="100">
          <template #default="{ row }">
            <el-tag v-if="row.fetchStatus === 0" type="success">正常</el-tag>
            <el-tag v-else-if="row.fetchStatus === 1" type="warning"
              >抓取中</el-tag
            >
            <el-tag v-else type="danger">失败</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="260" fixed="right">
          <template #default="{ row }">
            <el-button
              size="small"
              type="primary"
              link
              @click="handleTriggerFetch(row)"
            >
              立即抓取
            </el-button>
            <el-button
              size="small"
              type="primary"
              link
              @click="handleEdit(row)"
            >
              编辑
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

      <!-- 分页 -->
      <el-pagination
        v-model:current-page="pagination.page"
        v-model:page-size="pagination.pageSize"
        :total="pagination.total"
        :page-sizes="[10, 20, 50, 100]"
        layout="total, sizes, prev, pager, next, jumper"
        style="justify-content: flex-end; margin-top: 20px"
        @current-change="fetchData"
        @size-change="fetchData"
      />
    </el-card>

    <!-- 新增/编辑对话框 -->
    <el-dialog
      v-model="dialogVisible"
      :title="isEdit ? '编辑RSS源' : '新增RSS源'"
      width="600px"
      :close-on-click-modal="false"
    >
      <el-form
        ref="formRef"
        :model="formData"
        :rules="formRules"
        label-width="140px"
      >
        <el-form-item label="名称" prop="name">
          <el-input v-model="formData.name" placeholder="请输入RSS源名称" />
        </el-form-item>
        <el-form-item label="URL" prop="url">
          <el-input
            v-model="formData.url"
            placeholder="请输入RSS Feed URL"
            clearable
          />
        </el-form-item>
        <el-form-item label="是否启用" prop="isEnabled">
          <el-switch v-model="formData.isEnabled" />
        </el-form-item>
        <el-form-item label="抓取间隔(分钟)" prop="fetchIntervalMinutes">
          <el-input-number
            v-model="formData.fetchIntervalMinutes"
            :min="1"
            :max="1440"
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item label="最大条目数" prop="maxItems">
          <el-input-number
            v-model="formData.maxItems"
            :min="1"
            :max="1000"
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item label="搜索关键词">
          <el-input
            v-model="formData.query"
            placeholder="可选，用于过滤RSS条目"
            clearable
          />
        </el-form-item>
        <el-form-item label="代理地址">
          <el-input
            v-model="formData.proxyUrl"
            placeholder="可选，例如：http://127.0.0.1:7890"
            clearable
          />
        </el-form-item>
        <el-form-item label="代理用户名">
          <el-input
            v-model="formData.proxyUsername"
            placeholder="可选"
            clearable
          />
        </el-form-item>
        <el-form-item label="代理密码">
          <el-input
            v-model="formData.proxyPassword"
            type="password"
            placeholder="可选"
            clearable
            show-password
          />
        </el-form-item>
        <el-form-item label="备注">
          <el-input
            v-model="formData.remark"
            type="textarea"
            :rows="2"
            placeholder="请输入备注信息"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button
          type="primary"
          :loading="submitLoading"
          @click="handleSubmit"
        >
          确定
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from "vue";
import {
  ElMessage,
  ElMessageBox,
  type FormInstance,
  type FormRules
} from "element-plus";
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

// 对话框
const dialogVisible = ref(false);
const isEdit = ref(false);
const submitLoading = ref(false);
const formRef = ref<FormInstance>();
const formData = reactive<CreateUpdateRssSourceDto>({
  name: "",
  url: "",
  proxyUrl: "",
  proxyUsername: "",
  proxyPassword: "",
  isEnabled: true,
  fetchIntervalMinutes: 5,
  maxItems: 50,
  query: "",
  remark: ""
});

const formRules: FormRules = {
  name: [{ required: true, message: "请输入名称", trigger: "blur" }],
  url: [
    { required: true, message: "请输入URL", trigger: "blur" },
    { type: "url", message: "请输入有效的URL", trigger: "blur" }
  ],
  fetchIntervalMinutes: [
    { required: true, message: "请输入抓取间隔", trigger: "blur" }
  ],
  maxItems: [{ required: true, message: "请输入最大条目数", trigger: "blur" }]
};

let currentEditId: number | null = null;

// 获取数据
const fetchData = async () => {
  loading.value = true;
  try {
    const result = await rssSourceApi.getList({
      pageIndex: pagination.page,
      pageSize: pagination.pageSize,
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
    proxyUrl: "",
    proxyUsername: "",
    proxyPassword: "",
    isEnabled: true,
    fetchIntervalMinutes: 5,
    maxItems: 50,
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
    proxyUrl: row.proxyUrl || "",
    proxyUsername: row.proxyUsername || "",
    proxyPassword: row.proxyPassword || "",
    isEnabled: row.isEnabled,
    fetchIntervalMinutes: row.fetchIntervalMinutes,
    maxItems: row.maxItems,
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

  submitLoading.value = true;
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
  } finally {
    submitLoading.value = false;
  }
};

// 删除
const handleDelete = async (row: RssSourceDto) => {
  try {
    await ElMessageBox.confirm(`确定要删除RSS源 "${row.name}" 吗？`, "提示", {
      confirmButtonText: "确定",
      cancelButtonText: "取消",
      type: "warning"
    });

    await rssSourceApi.delete(row.id);
    ElMessage.success("删除成功");
    fetchData();
  } catch (error: any) {
    if (error !== "cancel") {
      console.error("删除失败:", error);
      ElMessage.error(error.message || "删除失败");
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
