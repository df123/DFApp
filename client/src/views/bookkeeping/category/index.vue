<template>
  <div class="bookkeeping-category-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">记账分类管理</span>
          <el-button
            v-permission="['DFApp.Bookkeeping.Category.Create']"
            type="primary"
            @click="handleCreate"
          >
            <el-icon><Plus /></el-icon>
            新增分类
          </el-button>
        </div>
      </template>

      <!-- 搜索区域 -->
      <el-form :model="searchForm" inline>
        <el-form-item label="分类名称">
          <el-input
            v-model="searchForm.category"
            placeholder="请输入分类名称"
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
            v-permission="['DFApp.Bookkeeping.Category.Edit']"
            size="small"
            type="primary"
            link
            @click="handleEdit(row)"
          >
            编辑
          </el-button>
          <el-button
            v-permission="['DFApp.Bookkeeping.Category.Delete']"
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
        <el-form-item label="分类名称" prop="category">
          <el-input
            v-model="formData.category"
            placeholder="请输入分类名称"
            maxlength="50"
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
import { useBookkeepingCategoryApi } from "@/api/bookkeeping";
import type {
  BookkeepingCategoryDto,
  CreateUpdateBookkeepingCategoryDto
} from "@/types/api";

// API 服务
const bookkeepingCategoryApi = useBookkeepingCategoryApi();

// 响应式数据
const loading = ref(false);
const submitting = ref(false);
const dialogVisible = ref(false);
const tableRef = ref();
const formRef = ref<FormInstance>();

// 搜索表单
const searchForm = reactive({
  category: ""
});

// 分页数据
const pagination = reactive({
  pageSize: 10,
  currentPage: 1,
  total: 0
});

// 表格数据
const tableData = ref<BookkeepingCategoryDto[]>([]);

// 表单数据
const formData = reactive<CreateUpdateBookkeepingCategoryDto>({
  category: ""
});

// 当前编辑的ID
const currentEditId = ref<number | null>(null);

// 计算属性
const dialogTitle = computed(() =>
  currentEditId.value ? "编辑分类" : "新增分类"
);

// 表单验证规则
const formRules: FormRules = {
  category: [
    { required: true, message: "请输入分类名称", trigger: "blur" },
    { min: 1, max: 50, message: "分类名称长度1-50个字符", trigger: "blur" }
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
    label: "分类名称",
    prop: "category",
    minWidth: 150
  },
  {
    label: "创建时间",
    prop: "creationTime",
    width: 180,
    formatter: (row: BookkeepingCategoryDto) => {
      return row.creationTime
        ? new Date(row.creationTime).toLocaleString()
        : "-";
    }
  },
  {
    label: "修改时间",
    prop: "lastModificationTime",
    width: 180,
    formatter: (row: BookkeepingCategoryDto) => {
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
      filter: searchForm.category || undefined
    };

    const result = await bookkeepingCategoryApi.getCategories(params);
    tableData.value = result.items;
    pagination.total = result.totalCount;
  } catch (error) {
    console.error("加载记账分类数据失败:", error);
    ElMessage.error("加载记账分类数据失败");
  } finally {
    loading.value = false;
  }
};

const handleSearch = () => {
  pagination.currentPage = 1;
  loadTableData();
};

const handleReset = () => {
  searchForm.category = "";
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
    category: ""
  });
  dialogVisible.value = true;
};

const handleEdit = (row: BookkeepingCategoryDto) => {
  currentEditId.value = row.id;
  Object.assign(formData, {
    category: row.category
  });
  dialogVisible.value = true;
};

const handleDelete = async (row: BookkeepingCategoryDto) => {
  try {
    await ElMessageBox.confirm(
      `确定要删除分类 "${row.category}" 吗？`,
      "删除确认",
      {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning"
      }
    );

    await bookkeepingCategoryApi.deleteCategory(row.id);
    ElMessage.success("删除成功");
    loadTableData();
  } catch (error) {
    if (error !== "cancel") {
      console.error("删除记账分类失败:", error);
      ElMessage.error("删除记账分类失败");
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
      await bookkeepingCategoryApi.updateCategory(
        currentEditId.value,
        formData
      );
      ElMessage.success("更新成功");
    } else {
      await bookkeepingCategoryApi.createCategory(formData);
      ElMessage.success("创建成功");
    }

    dialogVisible.value = false;
    loadTableData();
  } catch (error) {
    console.error("保存记账分类失败:", error);
    ElMessage.error("保存记账分类失败");
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
.bookkeeping-category-container {
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
