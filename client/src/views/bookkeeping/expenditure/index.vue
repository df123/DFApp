<template>
  <div class="expenditure-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">支出管理</span>
          <el-button
            v-permission="['DFApp.Bookkeeping.Expenditure.Create']"
            type="primary"
            size="small"
            class="mobile-button"
            @click="handleCreate"
          >
            <el-icon><Plus /></el-icon>
            <span class="button-text">新增</span>
          </el-button>
        </div>
      </template>

      <!-- 搜索区域 -->
      <div class="search-container">
        <!-- 桌面端搜索表单 -->
        <el-form :model="searchForm" class="desktop-search-form" inline>
          <el-form-item label="关键词">
            <el-input
              v-model="searchForm.filter"
              placeholder="请输入关键词"
              clearable
              style="width: 200px"
              @keyup.enter="handleSearch"
            />
          </el-form-item>
          <el-form-item label="分类">
            <el-select
              v-model="searchForm.categoryId"
              placeholder="请选择分类"
              clearable
              style="width: 150px"
            >
              <el-option
                v-for="category in categories"
                :key="category.id"
                :label="category.category"
                :value="category.id"
              />
            </el-select>
          </el-form-item>
          <el-form-item label="归属">
            <el-select
              v-model="searchForm.isBelongToSelf"
              placeholder="请选择归属"
              clearable
              style="width: 120px"
            >
              <el-option label="个人" :value="true" />
              <el-option label="家庭" :value="false" />
            </el-select>
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

        <!-- 移动端搜索表单 -->
        <el-form :model="searchForm" class="mobile-search-form">
          <div class="search-row">
            <el-form-item label="关键词" class="search-item">
              <el-input
                v-model="searchForm.filter"
                placeholder="请输入关键词"
                clearable
                @keyup.enter="handleSearch"
              />
            </el-form-item>
          </div>
          <div class="search-row">
            <el-form-item label="分类" class="search-item">
              <el-select
                v-model="searchForm.categoryId"
                placeholder="请选择分类"
                clearable
                class="mobile-select"
              >
                <el-option
                  v-for="category in categories"
                  :key="category.id"
                  :label="category.category"
                  :value="category.id"
                />
              </el-select>
            </el-form-item>
            <el-form-item label="归属" class="search-item">
              <el-select
                v-model="searchForm.isBelongToSelf"
                placeholder="请选择归属"
                clearable
                class="mobile-select"
              >
                <el-option label="个人" :value="true" />
                <el-option label="家庭" :value="false" />
              </el-select>
            </el-form-item>
          </div>
          <div class="search-actions">
            <el-button
              type="primary"
              class="mobile-button"
              @click="handleSearch"
            >
              <el-icon><Search /></el-icon>
              <span class="button-text">搜索</span>
            </el-button>
            <el-button class="mobile-button" @click="handleReset">
              <el-icon><Refresh /></el-icon>
              <span class="button-text">重置</span>
            </el-button>
          </div>
        </el-form>
      </div>

      <!-- 桌面端数据表格 -->
      <div class="desktop-table">
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
              v-permission="['DFApp.Bookkeeping.Expenditure.Edit']"
              size="small"
              type="primary"
              link
              @click="handleEdit(row)"
            >
              编辑
            </el-button>
            <el-button
              v-permission="['DFApp.Bookkeeping.Expenditure.Delete']"
              size="small"
              type="danger"
              link
              @click="handleDelete(row)"
            >
              删除
            </el-button>
          </template>
        </pure-table>
      </div>

      <!-- 移动端卡片列表 -->
      <div class="mobile-list">
        <div v-if="loading" class="loading-container">
          <el-skeleton :rows="3" animated />
        </div>
        <div v-else-if="tableData.length === 0" class="empty-container">
          <el-empty description="暂无数据" />
        </div>
        <div v-else class="expenditure-list">
          <div
            v-for="item in tableData"
            :key="item.id"
            class="expenditure-item"
          >
            <div class="item-header">
              <span class="item-amount"
                >￥{{ item.expenditure.toFixed(2) }}</span
              >
              <el-tag
                :type="item.isBelongToSelf ? 'primary' : 'success'"
                size="small"
              >
                {{ item.isBelongToSelf ? "个人" : "家庭" }}
              </el-tag>
            </div>
            <div class="item-content">
              <div class="item-row">
                <span class="item-label">日期:</span>
                <span class="item-value">{{
                  item.expenditureDate
                    ? new Date(item.expenditureDate).toLocaleDateString()
                    : "-"
                }}</span>
              </div>
              <div class="item-row">
                <span class="item-label">分类:</span>
                <span class="item-value">{{
                  item.category?.category || "-"
                }}</span>
              </div>
              <div v-if="item.remark" class="item-row">
                <span class="item-label">备注:</span>
                <span class="item-value">{{ item.remark }}</span>
              </div>
            </div>
            <div class="item-actions">
              <el-button
                v-permission="['DFApp.Bookkeeping.Expenditure.Edit']"
                size="small"
                type="primary"
                class="edit-btn"
                style="flex: 1; min-width: 0"
                @click="handleEdit(item)"
              >
                编辑
              </el-button>
              <el-button
                v-permission="['DFApp.Bookkeeping.Expenditure.Delete']"
                size="small"
                type="danger"
                class="delete-btn"
                style="flex: 1; min-width: 0"
                @click="handleDelete(item)"
              >
                删除
              </el-button>
            </div>
          </div>
        </div>

        <!-- 移动端分页 -->
        <div v-if="tableData.length > 0" class="mobile-pagination">
          <el-pagination
            v-model:current-page="pagination.currentPage"
            v-model:page-size="pagination.pageSize"
            :total="pagination.total"
            :page-sizes="[10, 20, 50, 100]"
            layout="prev, pager, next"
            small
            @size-change="handleSizeChange"
            @current-change="handleCurrentChange"
          />
        </div>
      </div>

      <!-- 统计信息 -->
      <div class="statistics">
        <el-descriptions :column="isMobile ? 1 : 2" border>
          <el-descriptions-item label="本页合计">
            <span class="amount">￥{{ pageTotal.toFixed(2) }}</span>
          </el-descriptions-item>
          <el-descriptions-item label="总计">
            <span class="amount">￥{{ total.toFixed(2) }}</span>
          </el-descriptions-item>
        </el-descriptions>
      </div>
    </el-card>

    <!-- 新增/编辑对话框 -->
    <el-dialog
      v-model="dialogVisible"
      :title="dialogTitle"
      :width="isMobile ? '95%' : '600px'"
      :fullscreen="isMobile"
      :before-close="handleClose"
      class="mobile-dialog"
    >
      <el-form
        ref="formRef"
        :model="formData"
        :rules="formRules"
        :label-width="isMobile ? '80px' : '100px'"
        :label-position="isMobile ? 'top' : 'right'"
      >
        <el-form-item label="支出日期" prop="expenditureDate">
          <el-date-picker
            v-model="formData.expenditureDate"
            type="date"
            placeholder="请选择支出日期"
            value-format="YYYY-MM-DD"
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item label="支出金额" prop="expenditure">
          <el-input-number
            v-model="formData.expenditure"
            placeholder="请输入支出金额"
            :min="0"
            :precision="2"
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item label="分类" prop="categoryId">
          <el-select
            v-model="formData.categoryId"
            placeholder="请选择分类"
            style="width: 100%"
          >
            <el-option
              v-for="category in categories"
              :key="category.id"
              :label="category.category"
              :value="category.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="归属" prop="isBelongToSelf">
          <el-radio-group v-model="formData.isBelongToSelf">
            <el-radio :label="true">个人</el-radio>
            <el-radio :label="false">家庭</el-radio>
          </el-radio-group>
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
        <div class="dialog-footer">
          <el-button
            class="cancel-btn"
            :size="isMobile ? 'large' : 'default'"
            @click="handleClose"
            >取消</el-button
          >
          <el-button
            type="primary"
            :loading="submitting"
            :size="isMobile ? 'large' : 'default'"
            class="confirm-btn"
            @click="handleSubmit"
          >
            确定
          </el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, onUnmounted, computed } from "vue";
import {
  ElMessage,
  ElMessageBox,
  type FormInstance,
  type FormRules
} from "element-plus";
import { Plus, Search, Refresh } from "@element-plus/icons-vue";
import {
  useBookkeepingExpenditureApi,
  useBookkeepingCategoryApi
} from "@/api/bookkeeping";
import type {
  BookkeepingExpenditureDto,
  CreateUpdateBookkeepingExpenditureDto,
  BookkeepingCategoryDto
} from "@/types/api";

// API 服务
const expenditureApi = useBookkeepingExpenditureApi();
const categoryApi = useBookkeepingCategoryApi();

// 响应式数据
const loading = ref(false);
const submitting = ref(false);
const dialogVisible = ref(false);
const tableRef = ref();
const formRef = ref<FormInstance>();
const isMobile = ref(false);

// 检测是否为移动端
const checkIsMobile = () => {
  isMobile.value = window.innerWidth <= 768;
};

// 监听窗口大小变化
const handleResize = () => {
  checkIsMobile();
};

// 搜索表单
const searchForm = reactive({
  filter: "",
  categoryId: undefined as number | undefined,
  isBelongToSelf: undefined as boolean | undefined
});

// 分页数据
const pagination = reactive({
  pageSize: 10,
  currentPage: 1,
  total: 0
});

// 表格数据
const tableData = ref<BookkeepingExpenditureDto[]>([]);

// 分类数据
const categories = ref<BookkeepingCategoryDto[]>([]);

// 统计信息
const pageTotal = ref(0);
const total = ref(0);

// 表单数据 (use ref instead of reactive)
const formData = ref<CreateUpdateBookkeepingExpenditureDto>({
  expenditureDate: "",
  expenditure: 0,
  remark: "",
  isBelongToSelf: true,
  categoryId: 0
});

// 当前编辑的ID
const currentEditId = ref<number | null>(null);

// 计算属性
const dialogTitle = computed(() =>
  currentEditId.value ? "编辑支出" : "新增支出"
);

// 表单验证规则
const formRules: FormRules = {
  expenditureDate: [
    { required: true, message: "请选择支出日期", trigger: "change" }
  ],
  expenditure: [
    { required: true, message: "请输入支出金额", trigger: "blur" },
    { type: "number", min: 0.01, message: "支出金额必须大于0", trigger: "blur" }
  ],
  categoryId: [{ required: true, message: "请选择分类", trigger: "change" }],
  isBelongToSelf: [{ required: true, message: "请选择归属", trigger: "change" }]
};

// 表格列配置
const columns = ref([
  {
    label: "支出日期",
    prop: "expenditureDate",
    width: 120,
    formatter: (row: BookkeepingExpenditureDto) => {
      return row.expenditureDate
        ? new Date(row.expenditureDate).toLocaleDateString()
        : "-";
    }
  },
  {
    label: "支出金额",
    prop: "expenditure",
    width: 120,
    formatter: (row: BookkeepingExpenditureDto) => {
      return `￥${row.expenditure.toFixed(2)}`;
    }
  },
  {
    label: "归属",
    prop: "isBelongToSelf",
    width: 80,
    formatter: (row: BookkeepingExpenditureDto) => {
      return row.isBelongToSelf ? "个人" : "家庭";
    }
  },
  {
    label: "分类",
    prop: "category.category",
    minWidth: 120
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
    formatter: (row: BookkeepingExpenditureDto) => {
      return row.creationTime
        ? new Date(row.creationTime).toLocaleString()
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
const loadCategories = async () => {
  try {
    const result = await categoryApi.getCategories({ pageSize: 1000 });
    categories.value = result.items;
  } catch (error) {
    console.error("加载分类数据失败:", error);
    ElMessage.error("加载分类数据失败");
  }
};

const loadTableData = async () => {
  loading.value = true;
  try {
    const params = {
      pageIndex: pagination.currentPage,
      pageSize: pagination.pageSize,
      filter: searchForm.filter || undefined,
      categoryId: searchForm.categoryId,
      isBelongToSelf: searchForm.isBelongToSelf,
      sorting: "ExpenditureDate DESC"
    };

    const result = await expenditureApi.getExpenditures(params);
    tableData.value = result.items;
    pagination.total = result.totalCount;

    // 获取总计
    const totalAmount = await expenditureApi.getTotalExpenditure({
      filter: searchForm.filter || undefined,
      categoryId: searchForm.categoryId,
      isBelongToSelf: searchForm.isBelongToSelf
    });

    // 计算统计信息
    calculateStatistics(totalAmount);
  } catch (error) {
    console.error("加载支出数据失败:", error);
    ElMessage.error("加载支出数据失败");
  } finally {
    loading.value = false;
  }
};

const calculateStatistics = (totalAmount: number) => {
  pageTotal.value = tableData.value.reduce(
    (sum, item) => sum + item.expenditure,
    0
  );
  total.value = totalAmount;
};

const handleSearch = () => {
  pagination.currentPage = 1;
  loadTableData();
};

const handleReset = () => {
  searchForm.filter = "";
  searchForm.categoryId = undefined;
  searchForm.isBelongToSelf = undefined;
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
  Object.assign(formData.value, {
    expenditureDate: new Date().toISOString().split("T")[0],
    expenditure: 0,
    remark: "",
    isBelongToSelf: true,
    categoryId: 1
  });
  dialogVisible.value = true;
};

const handleEdit = (row: BookkeepingExpenditureDto) => {
  currentEditId.value = row.id;
  Object.assign(formData.value, {
    expenditureDate: row.expenditureDate,
    expenditure: row.expenditure,
    remark: row.remark || "",
    isBelongToSelf: row.isBelongToSelf,
    categoryId: row.categoryId
  });
  dialogVisible.value = true;
};

const handleDelete = async (row: BookkeepingExpenditureDto) => {
  try {
    await ElMessageBox.confirm(`确定要删除支出记录吗？`, "删除确认", {
      confirmButtonText: "确定",
      cancelButtonText: "取消",
      type: "warning"
    });

    await expenditureApi.deleteExpenditure(row.id);
    ElMessage.success("删除成功");
    loadTableData();
  } catch (error) {
    if (error !== "cancel") {
      console.error("删除支出记录失败:", error);
      ElMessage.error("删除支出记录失败");
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
      await expenditureApi.updateExpenditure(
        currentEditId.value,
        formData.value
      );
      ElMessage.success("更新成功");
    } else {
      console.log("Creating expenditure with data:", formData.value);
      await expenditureApi.createExpenditure(formData.value);
      ElMessage.success("创建成功");
    }

    dialogVisible.value = false;
    loadTableData();
  } catch (error) {
    console.error("保存支出记录失败:", error);
    ElMessage.error("保存支出记录失败");
  } finally {
    submitting.value = false;
  }
};

// 生命周期
onMounted(() => {
  checkIsMobile();
  window.addEventListener("resize", handleResize);
  loadCategories();
  loadTableData();
});

onUnmounted(() => {
  window.removeEventListener("resize", handleResize);
});
</script>

<style scoped>
/* 响应式布局 */
@media (width <= 768px) {
  .expenditure-container {
    padding: 10px;
  }

  .card-header {
    flex-direction: column;
    gap: 10px;
    align-items: flex-start;
  }

  .card-title {
    font-size: 14px;
  }

  .desktop-search-form {
    display: none;
  }

  .mobile-search-form {
    display: block;
  }

  .search-row {
    flex-direction: column;
  }

  .search-item {
    display: flex;
    align-items: center;
    justify-content: space-between;
    min-width: 100%;
    margin-bottom: 12px;
  }

  .mobile-search-form .search-item :deep(.el-form-item__label) {
    flex-shrink: 0;
    width: 80px;
    margin-right: 12px;
    text-align: left;
  }

  .mobile-search-form .search-item :deep(.el-form-item__content) {
    flex: 1;
    text-align: right;
  }

  .mobile-search-form .search-item :deep(.el-input),
  .mobile-search-form .search-item :deep(.el-select) {
    width: 100%;
  }

  .search-actions {
    justify-content: center;
  }

  .desktop-table {
    display: none;
  }

  .mobile-list {
    display: block;
  }

  .item-header {
    flex-direction: column;
    gap: 8px;
    align-items: flex-start;
  }

  .item-actions {
    gap: 12px;
    justify-content: center;
  }

  .edit-btn,
  .delete-btn {
    flex: 1;
    width: 100%;
    min-width: 0;
  }

  .edit-btn :deep(.el-button),
  .delete-btn :deep(.el-button) {
    width: 100%;
    min-width: 0;
    margin-left: 0 !important;
  }

  /* 重置Element Plus按钮的默认margin */
  .item-actions :deep(.el-button + .el-button) {
    margin-left: 0 !important;
  }

  .statistics {
    padding: 12px;
  }

  .dialog-footer {
    flex-direction: row;
    gap: 12px;
  }

  .cancel-btn,
  .confirm-btn {
    flex: 1;
  }
}

@media (width <= 480px) {
  .expenditure-container {
    padding: 5px;
  }

  .expenditure-item {
    padding: 12px;
  }

  .item-amount {
    font-size: 16px;
  }

  .item-actions {
    flex-direction: column;
    gap: 8px;
  }

  .edit-btn,
  .delete-btn {
    flex: none;
    width: 100%;
  }

  .edit-btn :deep(.el-button),
  .delete-btn :deep(.el-button) {
    width: 100%;
    margin-left: 0 !important;
  }

  /* 重置Element Plus按钮的默认margin */
  .item-actions :deep(.el-button + .el-button) {
    margin-left: 0 !important;
  }
}

.expenditure-container {
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

.statistics {
  padding: 16px;
  margin-top: 20px;
  background-color: #f5f7fa;
  border-radius: 4px;
}

.amount {
  font-weight: bold;
  color: #e6a23c;
}

/* 移动端适配样式 */
.mobile-button {
  display: flex;
  gap: 4px;
  align-items: center;
}

.button-text {
  display: inline;
}

/* 搜索区域 */
.search-container {
  margin-bottom: 16px;
}

/* 桌面端搜索表单 */
.desktop-search-form {
  display: block;
}

.mobile-search-form {
  display: none;
}

.search-row {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 8px;
}

.search-item {
  flex: 1;
  min-width: 200px;
  margin-bottom: 8px;
}

.mobile-select {
  width: 100%;
}

/* 移动端搜索表单项布局 */
.mobile-search-form .search-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  min-width: 100%;
  margin-bottom: 12px;
}

.mobile-search-form .search-item :deep(.el-form-item__label) {
  flex-shrink: 0;
  width: 80px;
  margin-right: 12px;
  text-align: left;
}

.mobile-search-form .search-item :deep(.el-form-item__content) {
  flex: 1;
  text-align: right;
}

.mobile-search-form .search-item :deep(.el-input),
.mobile-search-form .search-item :deep(.el-select) {
  width: 100%;
}

.search-actions {
  display: flex;
  gap: 8px;
  justify-content: flex-end;
  margin-top: 8px;
}

/* 桌面端表格 */
.desktop-table {
  display: block;
}

.mobile-list {
  display: none;
}

/* 移动端卡片列表 */
.expenditure-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.expenditure-item {
  padding: 16px;
  background-color: #fff;
  border: 1px solid #e4e7ed;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgb(0 0 0 / 5%);
}

.item-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 12px;
}

.item-amount {
  font-size: 18px;
  font-weight: bold;
  color: #e6a23c;
}

.item-content {
  margin-bottom: 12px;
}

.item-row {
  display: flex;
  margin-bottom: 6px;
}

.item-label {
  min-width: 50px;
  font-size: 14px;
  color: #909399;
}

.item-value {
  flex: 1;
  font-size: 14px;
}

.item-actions {
  display: flex;
  gap: 8px;
  justify-content: flex-end;
}

.edit-btn {
  flex: 1;
  width: 100%;
  min-width: 0;
}

.edit-btn :deep(.el-button) {
  width: 100%;
  min-width: 0;
  margin-left: 0 !important;
}

.delete-btn {
  flex: 1;
  width: 100%;
  min-width: 0;
}

.delete-btn :deep(.el-button) {
  width: 100%;
  min-width: 0;
  margin-left: 0 !important;
}

/* 重置Element Plus按钮的默认margin */
.item-actions :deep(.el-button + .el-button) {
  margin-left: 0 !important;
}

.loading-container,
.empty-container {
  padding: 20px;
  text-align: center;
}

.mobile-pagination {
  display: flex;
  justify-content: center;
  margin-top: 16px;
}

/* 对话框适配 */
.mobile-dialog {
  padding: 0;
}

.dialog-footer {
  display: flex;
  gap: 8px;
  justify-content: flex-end;
}

.cancel-btn {
  flex: 1;
}

.confirm-btn {
  flex: 1;
}
</style>
