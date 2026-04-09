<template>
  <div class="configuration-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">配置管理</span>
          <el-button
            v-permission="['DFApp.ConfigurationInfo.Create']"
            type="primary"
            @click="handleCreate"
          >
            <el-icon><Plus /></el-icon>
            新增配置
          </el-button>
        </div>
      </template>

      <!-- 搜索区域 -->
      <el-form :model="searchForm" inline>
        <el-form-item label="模块名称">
          <el-input
            v-model="searchForm.moduleName"
            placeholder="请输入模块名称"
            clearable
            @keyup.enter="handleSearch"
          />
        </el-form-item>
        <el-form-item label="配置名称">
          <el-input
            v-model="searchForm.configurationName"
            placeholder="请输入配置名称"
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
            v-permission="['DFApp.ConfigurationInfo.Edit']"
            size="small"
            type="primary"
            link
            @click="handleEdit(row)"
          >
            编辑
          </el-button>
          <el-button
            v-permission="['DFApp.ConfigurationInfo.Delete']"
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
      width="600px"
      :before-close="handleClose"
    >
      <el-form
        ref="formRef"
        :model="formData"
        :rules="formRules"
        label-width="120px"
      >
        <el-form-item label="模块名称" prop="moduleName">
          <el-input
            v-model="formData.moduleName"
            placeholder="请输入模块名称"
            maxlength="100"
            show-word-limit
          />
        </el-form-item>
        <el-form-item label="配置名称" prop="configurationName">
          <el-input
            v-model="formData.configurationName"
            placeholder="请输入配置名称"
            maxlength="100"
            show-word-limit
          />
        </el-form-item>
        <el-form-item label="配置值" prop="configurationValue">
          <el-input
            v-model="formData.configurationValue"
            placeholder="请输入配置值"
            type="textarea"
            :rows="3"
            maxlength="500"
            show-word-limit
          />
        </el-form-item>
        <el-form-item label="备注">
          <el-input
            v-model="formData.remark"
            placeholder="请输入备注信息"
            type="textarea"
            :rows="2"
            maxlength="200"
            show-word-limit
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
import { useConfigurationApi } from "@/api/configuration";
import type {
  ConfigurationInfoDto,
  CreateUpdateConfigurationInfoDto
} from "@/types/api";

// API 服务
const configurationApi = useConfigurationApi();

// 响应式数据
const loading = ref(false);
const submitting = ref(false);
const dialogVisible = ref(false);
const tableRef = ref();
const formRef = ref<FormInstance>();

// 搜索表单
const searchForm = reactive({
  moduleName: "",
  configurationName: ""
});

// 分页数据
const pagination = reactive({
  pageSize: 10,
  currentPage: 1,
  total: 0
});

// 表格数据
const tableData = ref<ConfigurationInfoDto[]>([]);

// 表单数据
const formData = reactive<CreateUpdateConfigurationInfoDto>({
  moduleName: "",
  configurationName: "",
  configurationValue: "",
  remark: ""
});

// 当前编辑的ID
const currentEditId = ref<number | null>(null);

// 计算属性
const dialogTitle = computed(() =>
  currentEditId.value ? "编辑配置" : "新增配置"
);

// 表单验证规则
const formRules: FormRules = {
  moduleName: [{ required: false, message: "请输入模块名称", trigger: "blur" }],
  configurationName: [
    { required: true, message: "请输入配置名称", trigger: "blur" }
  ],
  configurationValue: [
    { required: true, message: "请输入配置值", trigger: "blur" }
  ]
};

// 表格列配置
const columns = ref([
  {
    label: "ID",
    prop: "id",
    width: 80
  },
  {
    label: "模块名称",
    prop: "moduleName",
    minWidth: 120
  },
  {
    label: "配置名称",
    prop: "configurationName",
    minWidth: 150
  },
  {
    label: "配置值",
    prop: "configurationValue",
    minWidth: 200,
    showOverflowTooltip: true
  },
  {
    label: "备注",
    prop: "remark",
    minWidth: 150,
    showOverflowTooltip: true
  },
  {
    label: "创建时间",
    prop: "creationTime",
    width: 180,
    formatter: (row: any) => {
      return row.creationTime
        ? new Date(row.creationTime).toLocaleString()
        : "-";
    }
  },
  {
    label: "修改时间",
    prop: "lastModificationTime",
    width: 180,
    formatter: (row: any) => {
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
        searchForm.moduleName || searchForm.configurationName
          ? `${searchForm.moduleName} ${searchForm.configurationName}`
          : undefined
    };

    const result = await configurationApi.getConfigurations(params);
    tableData.value = result.items;
    pagination.total = result.totalCount;
  } catch (error) {
    console.error("加载配置数据失败:", error);
    ElMessage.error("加载配置数据失败");
  } finally {
    loading.value = false;
  }
};

const handleSearch = () => {
  pagination.currentPage = 1;
  loadTableData();
};

const handleReset = () => {
  searchForm.moduleName = "";
  searchForm.configurationName = "";
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
    moduleName: "",
    configurationName: "",
    configurationValue: "",
    remark: ""
  });
  dialogVisible.value = true;
};

const handleEdit = (row: ConfigurationInfoDto) => {
  currentEditId.value = row.id;
  Object.assign(formData, {
    moduleName: row.moduleName,
    configurationName: row.configurationName,
    configurationValue: row.configurationValue,
    remark: row.remark || ""
  });
  dialogVisible.value = true;
};

const handleDelete = async (row: ConfigurationInfoDto) => {
  try {
    await ElMessageBox.confirm(
      `确定要删除配置 "${row.configurationName}" 吗？`,
      "删除确认",
      {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning"
      }
    );

    await configurationApi.deleteConfiguration(row.id);
    ElMessage.success("删除成功");
    loadTableData();
  } catch (error) {
    if (error !== "cancel") {
      console.error("删除配置失败:", error);
      ElMessage.error("删除配置失败");
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
      await configurationApi.updateConfiguration(currentEditId.value, formData);
      ElMessage.success("更新成功");
    } else {
      await configurationApi.createConfiguration(formData);
      ElMessage.success("创建成功");
    }

    dialogVisible.value = false;
    loadTableData();
  } catch (error) {
    console.error("保存配置失败:", error);
    ElMessage.error("保存配置失败");
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
.configuration-container {
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
