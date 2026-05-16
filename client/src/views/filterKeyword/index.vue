<template>
  <div class="keyword-filter-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">关键词过滤规则管理</span>
          <el-button type="primary" @click="handleCreate">新增规则</el-button>
        </div>
      </template>

      <!-- 搜索和筛选 -->
      <div class="search-container">
        <el-row :gutter="20">
          <el-col :span="6">
            <el-input
              v-model="searchKeyword"
              placeholder="搜索关键词"
              clearable
              @clear="handleSearch"
              @keyup.enter="handleSearch"
            >
              <template #append>
                <el-button @click="handleSearch">
                  <el-icon><Search /></el-icon>
                </el-button>
              </template>
            </el-input>
          </el-col>
          <el-col :span="6">
            <el-select
              v-model="searchFilterType"
              placeholder="过滤类型"
              clearable
              @change="handleSearch"
            >
              <el-option label="黑名单" :value="0" />
              <el-option label="白名单" :value="1" />
            </el-select>
          </el-col>
          <el-col :span="6">
            <el-select
              v-model="searchMatchMode"
              placeholder="匹配模式"
              clearable
              @change="handleSearch"
            >
              <el-option label="包含" :value="0" />
              <el-option label="开头" :value="1" />
              <el-option label="结尾" :value="2" />
              <el-option label="完全匹配" :value="3" />
              <el-option label="正则表达式" :value="4" />
            </el-select>
          </el-col>
          <el-col :span="6">
            <el-select
              v-model="searchEnabled"
              placeholder="启用状态"
              clearable
              @change="handleSearch"
            >
              <el-option label="已启用" :value="true" />
              <el-option label="已禁用" :value="false" />
            </el-select>
          </el-col>
        </el-row>
      </div>

      <!-- 数据表格 -->
      <div class="table-container">
        <el-table
          v-loading="loading"
          :data="tableData"
          stripe
          style="width: 100%"
        >
          <el-table-column prop="keyword" label="关键词" min-width="150" />
          <el-table-column prop="matchMode" label="匹配模式" width="120">
            <template #default="scope">
              <el-tag :type="getMatchModeTagType(scope.row.matchMode)">
                {{ getMatchModeText(scope.row.matchMode) }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="filterType" label="过滤类型" width="100">
            <template #default="scope">
              <el-tag :type="scope.row.filterType === 0 ? 'danger' : 'success'">
                {{ scope.row.filterType === 0 ? "黑名单" : "白名单" }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="isEnabled" label="启用状态" width="100">
            <template #default="scope">
              <el-switch
                v-model="scope.row.isEnabled"
                @change="handleToggleStatus(scope.row)"
              />
            </template>
          </el-table-column>
          <el-table-column prop="priority" label="优先级" width="100">
            <template #default="scope">
              <el-tag :type="getPriorityTagType(scope.row.priority)">
                {{ scope.row.priority }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column
            prop="isCaseSensitive"
            label="大小写敏感"
            width="120"
          >
            <template #default="scope">
              <el-tag :type="scope.row.isCaseSensitive ? 'warning' : 'info'">
                {{ scope.row.isCaseSensitive ? "是" : "否" }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column
            prop="remark"
            label="备注"
            min-width="150"
            show-overflow-tooltip
          />
          <el-table-column prop="creationTime" label="创建时间" width="180" />
          <el-table-column label="操作" width="200" fixed="right">
            <template #default="scope">
              <el-button size="small" @click="handleEdit(scope.row)">
                编辑
              </el-button>
              <el-button
                size="small"
                type="danger"
                @click="handleDelete(scope.row)"
              >
                删除
              </el-button>
              <el-button
                size="small"
                type="primary"
                link
                @click="handleTest(scope.row)"
              >
                测试
              </el-button>
            </template>
          </el-table-column>
        </el-table>

        <!-- 分页 -->
        <div class="pagination-container">
          <el-pagination
            v-model:current-page="currentPage"
            v-model:page-size="pageSize"
            :total="total"
            :page-sizes="[10, 20, 50, 100]"
            layout="total, sizes, prev, pager, next, jumper"
            @size-change="handleSizeChange"
            @current-change="handleCurrentChange"
          />
        </div>
      </div>
    </el-card>

    <!-- 编辑对话框 -->
    <el-dialog
      v-model="dialogVisible"
      :title="dialogTitle"
      width="600px"
      :close-on-click-modal="false"
    >
      <el-form
        ref="formRef"
        :model="formData"
        :rules="formRules"
        label-width="120px"
      >
        <el-form-item label="关键词" prop="keyword">
          <el-input
            v-model="formData.keyword"
            placeholder="请输入关键词"
            :rows="formData.matchMode === 4 ? 3 : 1"
            :type="formData.matchMode === 4 ? 'textarea' : 'text'"
          />
          <div v-if="formData.matchMode === 4" class="form-tips">
            正则表达式模式，请确保表达式语法正确
          </div>
        </el-form-item>
        <el-form-item label="匹配模式" prop="matchMode">
          <el-select v-model="formData.matchMode" placeholder="请选择匹配模式">
            <el-option label="包含" :value="0" />
            <el-option label="以...开头" :value="1" />
            <el-option label="以...结尾" :value="2" />
            <el-option label="完全匹配" :value="3" />
            <el-option label="正则表达式" :value="4" />
          </el-select>
        </el-form-item>
        <el-form-item label="过滤类型" prop="filterType">
          <el-select v-model="formData.filterType" placeholder="请选择过滤类型">
            <el-option label="黑名单（匹配到的文件将被过滤）" :value="0" />
            <el-option label="白名单（只有匹配到的文件才会保留）" :value="1" />
          </el-select>
        </el-form-item>
        <el-form-item label="优先级" prop="priority">
          <el-input-number
            v-model="formData.priority"
            :min="0"
            :max="999"
            controls-position="right"
          />
          <div class="form-tips">数字越小优先级越高，建议值：0-999</div>
        </el-form-item>
        <el-form-item label="是否启用" prop="isEnabled">
          <el-switch v-model="formData.isEnabled" />
        </el-form-item>
        <el-form-item label="大小写敏感" prop="isCaseSensitive">
          <el-switch v-model="formData.isCaseSensitive" />
        </el-form-item>
        <el-form-item label="备注" prop="remark">
          <el-input
            v-model="formData.remark"
            type="textarea"
            :rows="3"
            placeholder="请输入备注信息（可选）"
            maxlength="500"
            show-word-limit
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <span class="dialog-footer">
          <el-button @click="dialogVisible = false">取消</el-button>
          <el-button type="primary" :loading="submitting" @click="handleSubmit">
            确定
          </el-button>
        </span>
      </template>
    </el-dialog>

    <!-- 测试对话框 -->
    <el-dialog
      v-model="testDialogVisible"
      title="过滤规则测试"
      width="700px"
      :close-on-click-modal="false"
    >
      <div class="test-container">
        <el-input
          v-model="testFileName"
          placeholder="请输入要测试的文件名"
          style="margin-bottom: 20px"
          @keyup.enter="handleTestFile"
        >
          <template #append>
            <el-button @click="handleTestFile">测试</el-button>
          </template>
        </el-input>

        <div v-if="testResult" class="test-result">
          <el-alert
            :title="testResult.shouldFilter ? '文件将被过滤' : '文件将被保留'"
            :type="testResult.shouldFilter ? 'error' : 'success'"
            :closable="false"
            style="margin-bottom: 20px"
          />

          <div v-if="testResult.matchingRules.length > 0">
            <h4>匹配到的规则：</h4>
            <el-table :data="testResult.matchingRules" stripe>
              <el-table-column prop="keyword" label="关键词" width="150" />
              <el-table-column prop="matchMode" label="匹配模式" width="120">
                <template #default="scope">
                  {{ getMatchModeText(scope.row.matchMode) }}
                </template>
              </el-table-column>
              <el-table-column prop="filterType" label="过滤类型" width="100">
                <template #default="scope">
                  <el-tag
                    :type="scope.row.filterType === 0 ? 'danger' : 'success'"
                  >
                    {{ scope.row.filterType === 0 ? "黑名单" : "白名单" }}
                  </el-tag>
                </template>
              </el-table-column>
              <el-table-column prop="priority" label="优先级" width="80" />
              <el-table-column prop="matchedText" label="匹配文本" />
            </el-table>
          </div>

          <div v-else>
            <el-alert title="未匹配到任何规则" type="info" :closable="false" />
          </div>

          <div class="test-reason" style="margin-top: 20px">
            <strong>判断理由：</strong> {{ testResult.reason }}
          </div>
        </div>
      </div>
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
import { Search } from "@element-plus/icons-vue";
import { keywordFilterApi } from "@/api/keywordFilter";
import type {
  KeywordFilterRuleDto,
  CreateUpdateKeywordFilterRuleDto,
  KeywordFilterTestResultDto
} from "@/types/business";

// 搜索条件
const searchKeyword = ref("");
const searchFilterType = ref<number | null>(null);
const searchMatchMode = ref<number | null>(null);
const searchEnabled = ref<boolean | null>(null);

// 表格数据
const loading = ref(false);
const tableData = ref<KeywordFilterRuleDto[]>([]);
const currentPage = ref(1);
const pageSize = ref(20);
const total = ref(0);

// 对话框
const dialogVisible = ref(false);
const dialogTitle = ref("");
const editingId = ref<number | null>(null);
const formRef = ref<FormInstance>();
const submitting = ref(false);
const formData = reactive<CreateUpdateKeywordFilterRuleDto>({
  keyword: "",
  matchMode: 0,
  filterType: 0,
  priority: 100,
  isEnabled: true,
  isCaseSensitive: false,
  remark: ""
});

// 测试对话框
const testDialogVisible = ref(false);
const testFileName = ref("");
const testResult = ref<KeywordFilterTestResultDto | null>(null);

// 表单验证规则
const formRules: FormRules = {
  keyword: [
    { required: true, message: "请输入关键词", trigger: "blur" },
    { max: 200, message: "关键词长度不能超过200个字符", trigger: "blur" }
  ],
  matchMode: [{ required: true, message: "请选择匹配模式", trigger: "change" }],
  filterType: [
    { required: true, message: "请选择过滤类型", trigger: "change" }
  ],
  priority: [
    { required: true, message: "请输入优先级", trigger: "blur" },
    {
      type: "number",
      min: 0,
      max: 999,
      message: "优先级必须在0-999之间",
      trigger: "blur"
    }
  ]
};

// 获取匹配模式文本
const getMatchModeText = (mode: number): string => {
  const modes = ["包含", "开头", "结尾", "完全匹配", "正则表达式"];
  return modes[mode] || "未知";
};

// 获取匹配模式标签类型
const getMatchModeTagType = (mode: number): string => {
  const types = ["", "success", "warning", "info", "danger"];
  return types[mode] || "";
};

// 获取优先级标签类型
const getPriorityTagType = (priority: number): string => {
  if (priority <= 10) return "danger";
  if (priority <= 50) return "warning";
  if (priority <= 100) return "primary";
  return "info";
};

// 加载数据
const loadData = async () => {
  loading.value = true;
  try {
    const request = {
      filter: searchKeyword.value,
      pageIndex: currentPage.value,
      pageSize: pageSize.value
    };
    const response = await keywordFilterApi.getList(request);
    tableData.value = response.items;
    total.value = response.totalCount;
  } catch (error) {
    console.error("加载数据失败:", error);
    ElMessage.error("加载数据失败");
  } finally {
    loading.value = false;
  }
};

// 搜索
const handleSearch = () => {
  currentPage.value = 1;
  loadData();
};

// 分页大小改变
const handleSizeChange = (size: number) => {
  pageSize.value = size;
  loadData();
};

// 当前页改变
const handleCurrentChange = (page: number) => {
  currentPage.value = page;
  loadData();
};

// 新增
const handleCreate = () => {
  dialogTitle.value = "新增过滤规则";
  editingId.value = null;
  Object.assign(formData, {
    keyword: "",
    matchMode: 0,
    filterType: 0,
    priority: 100,
    isEnabled: true,
    isCaseSensitive: false,
    remark: ""
  });
  dialogVisible.value = true;
};

// 编辑
const handleEdit = (row: KeywordFilterRuleDto) => {
  dialogTitle.value = "编辑过滤规则";
  editingId.value = row.id;
  Object.assign(formData, {
    keyword: row.keyword,
    matchMode: row.matchMode,
    filterType: row.filterType,
    priority: row.priority,
    isEnabled: row.isEnabled,
    isCaseSensitive: row.isCaseSensitive,
    remark: row.remark || ""
  });
  dialogVisible.value = true;
};

// 提交表单
const handleSubmit = async () => {
  if (!formRef.value) return;

  const valid = await formRef.value.validate();
  if (!valid) return;

  submitting.value = true;
  try {
    if (editingId.value === null) {
      // 新增
      await keywordFilterApi.create(formData);
      ElMessage.success("新增成功");
    } else {
      // 编辑
      await keywordFilterApi.update(editingId.value, formData);
      ElMessage.success("编辑成功");
    }
    dialogVisible.value = false;
    loadData();
  } catch (error) {
    console.error("保存失败:", error);
    ElMessage.error("保存失败");
  } finally {
    submitting.value = false;
  }
};

// 删除
const handleDelete = async (row: KeywordFilterRuleDto) => {
  try {
    await ElMessageBox.confirm("确定要删除这条规则吗？", "提示", {
      type: "warning"
    });
    await keywordFilterApi.delete(row.id);
    ElMessage.success("删除成功");
    loadData();
  } catch (error) {
    if (error !== "cancel") {
      console.error("删除失败:", error);
      ElMessage.error("删除失败");
    }
  }
};

// 切换启用状态
const handleToggleStatus = async (row: KeywordFilterRuleDto) => {
  try {
    await keywordFilterApi.toggleRule(row.id, row.isEnabled);
    ElMessage.success(row.isEnabled ? "已启用" : "已禁用");
  } catch (error) {
    console.error("切换状态失败:", error);
    ElMessage.error("切换状态失败");
    // 回滚状态
    row.isEnabled = !row.isEnabled;
  }
};

// 测试规则
const handleTest = (row: KeywordFilterRuleDto) => {
  testFileName.value = "";
  testResult.value = null;
  testDialogVisible.value = true;
  // 可以预设测试文件名
  testFileName.value = row.keyword + "_test.txt";
};

// 测试文件
const handleTestFile = async () => {
  if (!testFileName.value.trim()) {
    ElMessage.warning("请输入要测试的文件名");
    return;
  }

  try {
    testResult.value = await keywordFilterApi.testFilter(testFileName.value);
  } catch (error) {
    console.error("测试失败:", error);
    ElMessage.error("测试失败");
  }
};

// 生命周期
onMounted(() => {
  loadData();
});
</script>

<style scoped>
.keyword-filter-container {
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

.search-container {
  margin-bottom: 20px;
}

.table-container {
  margin-top: 20px;
}

.pagination-container {
  display: flex;
  justify-content: flex-end;
  margin-top: 20px;
}

.form-tips {
  margin-top: 4px;
  font-size: 12px;
  color: #909399;
}

.test-result {
  margin-top: 20px;
}

.test-reason {
  padding: 10px;
  background-color: #f5f7fa;
  border-radius: 4px;
}
</style>
