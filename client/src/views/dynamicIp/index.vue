<template>
  <div class="dynamic-ip-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">动态IP管理</span>
          <el-button
            v-permission="['DFApp.DynamicIP.Create']"
            type="primary"
            @click="handleCreate"
          >
            <el-icon><Plus /></el-icon>
            新增IP
          </el-button>
        </div>
      </template>

      <!-- 搜索区域 -->
      <el-form :model="searchForm" inline>
        <el-form-item label="IP地址">
          <el-input
            v-model="searchForm.ip"
            placeholder="请输入IP地址"
            clearable
            @keyup.enter="handleSearch"
          />
        </el-form-item>
        <el-form-item label="端口">
          <el-input
            v-model="searchForm.port"
            placeholder="请输入端口"
            clearable
            @keyup.enter="handleSearch"
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">
            <el-icon><Search /></el-icon>
            搜索
          </el-button>
          <el-button @click="handleReset">
            <el-icon><Refresh /></el-icon>
            重置
          </el-button>
        </el-form-item>
      </el-form>

      <!-- 数据表格 -->
      <pure-table
        ref="tableRef"
        :loading="loading"
        :data="tableData"
        :columns="columns"
        :pagination="pagination"
        @page-size-change="handleSizeChange"
        @page-current-change="handleCurrentChange"
      >
        <template #operation="{ row }">
          <el-button
            v-permission="['DFApp.DynamicIP.Edit']"
            size="small"
            type="primary"
            link
            @click="handleEdit(row)"
          >
            编辑
          </el-button>
          <el-button
            v-permission="['DFApp.DynamicIP.Delete']"
            size="small"
            type="danger"
            link
            @click="handleDelete(row)"
          >
            删除
          </el-button>
        </template>
      </pure-table>
    </el-card>

    <!-- 新增/编辑对话框 -->
    <el-dialog
      v-model="dialogVisible"
      :title="dialogTitle"
      width="500px"
      :before-close="handleClose"
    >
      <el-form
        ref="formRef"
        :model="formData"
        :rules="formRules"
        label-width="80px"
      >
        <el-form-item label="IP地址" prop="ip">
          <el-input
            v-model="formData.ip"
            placeholder="请输入IP地址"
            maxlength="15"
          />
        </el-form-item>
        <el-form-item label="端口" prop="port">
          <el-input
            v-model="formData.port"
            placeholder="请输入端口"
            maxlength="5"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="handleClose">取消</el-button>
        <el-button type="primary" :loading="submitting" @click="handleSubmit">
          确定
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed } from "vue";
import {
  ElMessage,
  ElMessageBox,
  type FormInstance,
  type FormRules
} from "element-plus";
import { Plus, Search, Refresh } from "@element-plus/icons-vue";
import { useDynamicIpApi } from "@/api/dynamicIp";
import type { DynamicIPDto, CreateUpdateDynamicIPDto } from "@/types/api";

// API 服务
const dynamicIpApi = useDynamicIpApi();

// 响应式数据
const loading = ref(false);
const submitting = ref(false);
const dialogVisible = ref(false);
const tableRef = ref();
const formRef = ref<FormInstance>();

// 搜索表单
const searchForm = reactive({
  ip: "",
  port: ""
});

// 分页数据
const pagination = reactive({
  pageSize: 10,
  currentPage: 1,
  total: 0
});

// 表格数据
const tableData = ref<DynamicIPDto[]>([]);

// 表单数据
const formData = reactive<CreateUpdateDynamicIPDto>({
  ip: "",
  port: ""
});

// 当前编辑的ID
const currentEditId = ref<string | null>(null);

// 计算属性
const dialogTitle = computed(() =>
  currentEditId.value ? "编辑动态IP" : "新增动态IP"
);

// 表单验证规则
const formRules: FormRules = {
  ip: [
    { required: true, message: "请输入IP地址", trigger: "blur" },
    {
      pattern: /^(\d{1,3}\.){3}\d{1,3}$/,
      message: "请输入正确的IP地址格式",
      trigger: "blur"
    }
  ],
  port: [
    { required: true, message: "请输入端口", trigger: "blur" },
    { pattern: /^\d+$/, message: "端口必须为数字", trigger: "blur" },
    { min: 1, max: 65535, message: "端口范围1-65535", trigger: "blur" }
  ]
};

// 表格列配置
const columns = ref([
  {
    label: "IP地址:端口",
    prop: "address",
    minWidth: 150,
    formatter: (row: DynamicIPDto) => `${row.ip}:${row.port}`
  },
  {
    label: "IP地址",
    prop: "ip",
    minWidth: 120
  },
  {
    label: "端口",
    prop: "port",
    width: 100
  },
  {
    label: "创建时间",
    prop: "creationTime",
    width: 180,
    formatter: (row: DynamicIPDto) => {
      return row.creationTime
        ? new Date(row.creationTime).toLocaleString()
        : "-";
    }
  },
  {
    label: "修改时间",
    prop: "lastModificationTime",
    width: 180,
    formatter: (row: DynamicIPDto) => {
      return row.lastModificationTime
        ? new Date(row.lastModificationTime).toLocaleString()
        : "-";
    }
  },
  {
    label: "操作",
    slot: "operation",
    fixed: "right",
    width: 120
  }
]);

// 方法
const loadTableData = async () => {
  loading.value = true;
  try {
    const params = {
      skipCount: (pagination.currentPage - 1) * pagination.pageSize,
      maxResultCount: pagination.pageSize,
      filter:
        searchForm.ip || searchForm.port
          ? `${searchForm.ip} ${searchForm.port}`
          : undefined
    };

    const result = await dynamicIpApi.getDynamicIPs(params);
    tableData.value = result.items;
    pagination.total = result.totalCount;
  } catch (error) {
    console.error("加载动态IP数据失败:", error);
    ElMessage.error("加载动态IP数据失败");
  } finally {
    loading.value = false;
  }
};

const handleSearch = () => {
  pagination.currentPage = 1;
  loadTableData();
};

const handleReset = () => {
  searchForm.ip = "";
  searchForm.port = "";
  pagination.currentPage = 1;
  loadTableData();
};

const handleSizeChange = (size: number) => {
  pagination.pageSize = size;
  pagination.currentPage = 1;
  loadTableData();
};

const handleCurrentChange = (page: number) => {
  pagination.currentPage = page;
  loadTableData();
};

const handleCreate = () => {
  currentEditId.value = null;
  Object.assign(formData, {
    ip: "",
    port: ""
  });
  dialogVisible.value = true;
};

const handleEdit = (row: DynamicIPDto) => {
  currentEditId.value = row.id;
  Object.assign(formData, {
    ip: row.ip,
    port: row.port
  });
  dialogVisible.value = true;
};

const handleDelete = async (row: DynamicIPDto) => {
  try {
    await ElMessageBox.confirm(
      `确定要删除IP地址 "${row.ip}:${row.port}" 吗？`,
      "删除确认",
      {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning"
      }
    );

    await dynamicIpApi.deleteDynamicIP(row.id);
    ElMessage.success("删除成功");
    loadTableData();
  } catch (error) {
    if (error !== "cancel") {
      console.error("删除动态IP失败:", error);
      ElMessage.error("删除动态IP失败");
    }
  }
};

const handleClose = () => {
  dialogVisible.value = false;
  formRef.value?.clearValidate();
};

const handleSubmit = async () => {
  if (!formRef.value) return;

  const valid = await formRef.value.validate();
  if (!valid) return;

  submitting.value = true;
  try {
    if (currentEditId.value) {
      await dynamicIpApi.updateDynamicIP(currentEditId.value, formData);
      ElMessage.success("更新成功");
    } else {
      await dynamicIpApi.createDynamicIP(formData);
      ElMessage.success("创建成功");
    }

    dialogVisible.value = false;
    loadTableData();
  } catch (error) {
    console.error("保存动态IP失败:", error);
    ElMessage.error("保存动态IP失败");
  } finally {
    submitting.value = false;
  }
};

// 生命周期
onMounted(() => {
  loadTableData();
});
</script>

<style scoped>
.dynamic-ip-container {
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
}
</style>
