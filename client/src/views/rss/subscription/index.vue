<template>
  <div class="rss-subscription-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">RSS订阅管理</span>
          <el-button type="primary" @click="handleCreate">新增订阅</el-button>
        </div>
      </template>

      <el-form :inline="true" :model="searchForm" class="search-form">
        <el-form-item label="关键词">
          <el-input
            v-model="searchForm.filter"
            placeholder="搜索名称或关键词"
            clearable
            @clear="handleSearch"
          />
        </el-form-item>
        <el-form-item label="RSS源">
          <el-select
            v-model="searchForm.rssSourceId"
            placeholder="全部"
            clearable
            style="width: 150px"
            @change="handleSearch"
          >
            <el-option
              v-for="source in rssSources"
              :key="source.id"
              :label="source.name"
              :value="source.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="状态">
          <el-select
            v-model="searchForm.isEnabled"
            placeholder="全部"
            clearable
            style="width: 150px"
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

      <el-table
        v-loading="loading"
        :data="tableData"
        stripe
        style="width: 100%"
      >
        <el-table-column
          prop="name"
          label="订阅名称"
          min-width="150"
          show-overflow-tooltip
        />
        <el-table-column
          prop="keywords"
          label="关键词"
          min-width="200"
          show-overflow-tooltip
        />
        <el-table-column prop="rssSourceName" label="RSS源" width="120" />
        <el-table-column label="状态" width="80">
          <template #default="{ row }">
            <el-tag :type="row.isEnabled ? 'success' : 'info'">
              {{ row.isEnabled ? "启用" : "禁用" }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="qualityFilter" label="质量" width="100">
          <template #default="{ row }">
            <span v-if="row.qualityFilter">{{ row.qualityFilter }}</span>
            <span v-else>-</span>
          </template>
        </el-table-column>
        <el-table-column prop="autoDownload" label="自动下载" width="100">
          <template #default="{ row }">
            <el-tag
              :type="row.autoDownload ? 'success' : 'warning'"
              size="small"
            >
              {{ row.autoDownload ? "是" : "否" }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="creationTime" label="创建时间" width="180">
          <template #default="{ row }">
            {{ formatDate(row.creationTime) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="220" fixed="right">
          <template #default="{ row }">
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
              :type="row.isEnabled ? 'warning' : 'success'"
              link
              @click="handleToggleEnable(row)"
            >
              {{ row.isEnabled ? "禁用" : "启用" }}
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

    <el-dialog
      v-model="showDialog"
      :title="isEdit ? '编辑订阅' : '新增订阅'"
      width="700px"
      :close-on-click-modal="false"
    >
      <el-form
        ref="formRef"
        :model="formData"
        :rules="formRules"
        label-width="120px"
      >
        <el-form-item label="订阅名称" prop="name">
          <el-input v-model="formData.name" placeholder="请输入订阅名称" />
        </el-form-item>
        <el-form-item label="关键词" prop="keywords">
          <el-input
            v-model="formData.keywords"
            type="textarea"
            :rows="2"
            placeholder="多个关键词用逗号分隔，如：葬送的芙莉莲,Sousou no Frieren"
          />
          <div class="form-tip">匹配RSS条目标题，多个关键词用逗号分隔</div>
        </el-form-item>
        <el-form-item label="RSS源">
          <el-select
            v-model="formData.rssSourceId"
            placeholder="不限制（全部RSS源）"
            clearable
            style="width: 100%"
          >
            <el-option
              v-for="source in rssSources"
              :key="source.id"
              :label="source.name"
              :value="source.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="是否启用">
          <el-switch v-model="formData.isEnabled" />
          <div class="form-tip">禁用的订阅不会参与匹配</div>
        </el-form-item>

        <el-divider>质量过滤</el-divider>

        <el-form-item label="质量过滤">
          <el-input
            v-model="formData.qualityFilter"
            placeholder="如：1080p,4K,2160p"
          />
          <div class="form-tip">匹配标题中包含指定质量的条目</div>
        </el-form-item>
        <el-form-item label="字幕组过滤">
          <el-input
            v-model="formData.subtitleGroupFilter"
            placeholder="多个字幕组用逗号分隔"
          />
          <div class="form-tip">匹配标题中包含指定字幕组的条目</div>
        </el-form-item>

        <el-divider>数值过滤</el-divider>

        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="最小做种者">
              <el-input-number
                v-model="formData.minSeeders"
                :min="0"
                controls-position="right"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="最大做种者">
              <el-input-number
                v-model="formData.maxSeeders"
                :min="0"
                controls-position="right"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
        </el-row>

        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="最小下载者">
              <el-input-number
                v-model="formData.minLeechers"
                :min="0"
                controls-position="right"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="最大下载者">
              <el-input-number
                v-model="formData.maxLeechers"
                :min="0"
                controls-position="right"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
        </el-row>

        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="最小下载数">
              <el-input-number
                v-model="formData.minDownloads"
                :min="0"
                controls-position="right"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="最大下载数">
              <el-input-number
                v-model="formData.maxDownloads"
                :min="0"
                controls-position="right"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
        </el-row>

        <el-divider>下载设置</el-divider>

        <el-form-item label="自动下载">
          <el-switch v-model="formData.autoDownload" />
          <div class="form-tip">匹配成功后自动添加到Aria2下载</div>
        </el-form-item>
        <el-form-item label="仅下载视频">
          <el-switch v-model="formData.videoOnly" />
          <div class="form-tip">仅适用于.torrent文件</div>
        </el-form-item>
        <el-form-item label="启用关键词过滤">
          <el-switch v-model="formData.enableKeywordFilter" />
          <div class="form-tip">根据关键词过滤规则过滤文件</div>
        </el-form-item>
        <el-form-item label="保存路径">
          <el-input
            v-model="formData.savePath"
            placeholder="可选，留空使用默认路径"
          />
        </el-form-item>

        <el-divider>有效期</el-divider>

        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="开始日期">
              <el-date-picker
                v-model="formData.startDate"
                type="datetime"
                placeholder="不限"
                value-format="YYYY-MM-DDTHH:mm:ss"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="结束日期">
              <el-date-picker
                v-model="formData.endDate"
                type="datetime"
                placeholder="不限"
                value-format="YYYY-MM-DDTHH:mm:ss"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item label="备注">
          <el-input
            v-model="formData.remark"
            type="textarea"
            :rows="2"
            placeholder="可选"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showDialog = false">取消</el-button>
        <el-button type="primary" :loading="submitting" @click="handleSubmit">
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
import { rssSubscriptionApi } from "@/api/rssSubscription";
import { rssSourceApi } from "@/api/rssSource";
import type {
  RssSubscriptionDto,
  RssSourceDto,
  CreateUpdateRssSubscriptionDto
} from "@/types/business";

const searchForm = reactive({
  filter: "",
  rssSourceId: undefined as number | undefined,
  isEnabled: undefined as boolean | undefined
});

const pagination = reactive({
  page: 1,
  pageSize: 20,
  total: 0
});

const loading = ref(false);
const tableData = ref<RssSubscriptionDto[]>([]);
const rssSources = ref<RssSourceDto[]>([]);

const showDialog = ref(false);
const isEdit = ref(false);
const submitting = ref(false);
const formRef = ref<FormInstance>();

const formData = reactive<CreateUpdateRssSubscriptionDto>({
  name: "",
  keywords: "",
  isEnabled: true,
  minSeeders: undefined,
  maxSeeders: undefined,
  minLeechers: undefined,
  maxLeechers: undefined,
  minDownloads: undefined,
  maxDownloads: undefined,
  qualityFilter: undefined,
  subtitleGroupFilter: undefined,
  autoDownload: true,
  videoOnly: false,
  enableKeywordFilter: false,
  savePath: undefined,
  rssSourceId: undefined,
  startDate: undefined,
  endDate: undefined,
  remark: undefined
});

const formRules: FormRules = {
  name: [{ required: true, message: "请输入订阅名称", trigger: "blur" }],
  keywords: [{ required: true, message: "请输入关键词", trigger: "blur" }]
};

let currentEditId: number | null = null;

const fetchData = async () => {
  loading.value = true;
  try {
    const result = await rssSubscriptionApi.getList({
      skipCount: (pagination.page - 1) * pagination.pageSize,
      maxResultCount: pagination.pageSize,
      sorting: "CreationTime desc",
      ...searchForm
    });
    tableData.value = result.items || [];
    pagination.total = result.totalCount || 0;
  } catch (error) {
    console.error("获取订阅列表失败:", error);
    ElMessage.error("获取订阅列表失败");
  } finally {
    loading.value = false;
  }
};

const fetchRssSources = async () => {
  try {
    const result = await rssSourceApi.getList({
      skipCount: 0,
      maxResultCount: 1000
    });
    rssSources.value = result.items || [];
  } catch (error) {
    console.error("获取RSS源列表失败:", error);
  }
};

const handleSearch = () => {
  pagination.page = 1;
  fetchData();
};

const handleReset = () => {
  searchForm.filter = "";
  searchForm.rssSourceId = undefined;
  searchForm.isEnabled = undefined;
  pagination.page = 1;
  fetchData();
};

const handleCreate = () => {
  isEdit.value = false;
  currentEditId = null;
  Object.assign(formData, {
    name: "",
    keywords: "",
    isEnabled: true,
    minSeeders: undefined,
    maxSeeders: undefined,
    minLeechers: undefined,
    maxLeechers: undefined,
    minDownloads: undefined,
    maxDownloads: undefined,
    qualityFilter: undefined,
    subtitleGroupFilter: undefined,
    autoDownload: true,
    videoOnly: false,
    enableKeywordFilter: false,
    savePath: undefined,
    rssSourceId: undefined,
    startDate: undefined,
    endDate: undefined,
    remark: undefined
  });
  showDialog.value = true;
};

const handleEdit = (row: RssSubscriptionDto) => {
  isEdit.value = true;
  currentEditId = row.id;
  Object.assign(formData, {
    name: row.name,
    keywords: row.keywords,
    isEnabled: row.isEnabled,
    minSeeders: row.minSeeders,
    maxSeeders: row.maxSeeders,
    minLeechers: row.minLeechers,
    maxLeechers: row.maxLeechers,
    minDownloads: row.minDownloads,
    maxDownloads: row.maxDownloads,
    qualityFilter: row.qualityFilter,
    subtitleGroupFilter: row.subtitleGroupFilter,
    autoDownload: row.autoDownload,
    videoOnly: row.videoOnly,
    enableKeywordFilter: row.enableKeywordFilter,
    savePath: row.savePath,
    rssSourceId: row.rssSourceId,
    startDate: row.startDate,
    endDate: row.endDate,
    remark: row.remark
  });
  showDialog.value = true;
};

const handleSubmit = async () => {
  if (!formRef.value) return;

  try {
    await formRef.value.validate();
    submitting.value = true;

    if (isEdit.value && currentEditId) {
      console.log("更新订阅数据:", currentEditId);
      await rssSubscriptionApi.update(currentEditId, formData);
      ElMessage.success("更新成功");
    } else {
      await rssSubscriptionApi.create(formData);
      ElMessage.success("创建成功");
    }

    showDialog.value = false;
    fetchData();
  } catch (error: any) {
    console.error("提交失败:", error);
    ElMessage.error(error.message || "提交失败");
  } finally {
    submitting.value = false;
  }
};

const handleToggleEnable = async (row: RssSubscriptionDto) => {
  try {
    await rssSubscriptionApi.toggleEnable(row.id);
    ElMessage.success(`${row.isEnabled ? "禁用" : "启用"}成功`);
    fetchData();
  } catch (error) {
    console.error("切换状态失败:", error);
    ElMessage.error("切换状态失败");
  }
};

const handleDelete = async (row: RssSubscriptionDto) => {
  try {
    await ElMessageBox.confirm(`确定要删除订阅 "${row.name}" 吗？`, "提示", {
      confirmButtonText: "确定",
      cancelButtonText: "取消",
      type: "warning"
    });

    await rssSubscriptionApi.delete(row.id);
    ElMessage.success("删除成功");
    fetchData();
  } catch (error) {
    if (error !== "cancel") {
      console.error("删除失败:", error);
      ElMessage.error("删除失败");
    }
  }
};

const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleString("zh-CN");
};

onMounted(() => {
  fetchRssSources();
  fetchData();
});
</script>

<style scoped>
.rss-subscription-container {
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

.form-tip {
  margin-top: 5px;
  font-size: 12px;
  color: #909399;
}
</style>
