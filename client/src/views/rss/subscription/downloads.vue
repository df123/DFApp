<template>
  <div class="rss-subscription-downloads-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">订阅下载记录</span>
          <div>
            <el-button type="danger" @click="handleClearAll"
              >清空记录</el-button
            >
          </div>
        </div>
      </template>

      <el-form :inline="true" :model="searchForm" class="search-form">
        <el-form-item label="订阅">
          <el-select
            v-model="searchForm.subscriptionId"
            placeholder="全部"
            clearable
            filterable
            style="width: 150px"
            @change="handleSearch"
          >
            <el-option
              v-for="sub in subscriptions"
              :key="sub.id"
              :label="sub.name"
              :value="sub.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="关键词">
          <el-input
            v-model="searchForm.filter"
            placeholder="搜索标题"
            clearable
            @clear="handleSearch"
          />
        </el-form-item>
        <el-form-item label="下载状态">
          <el-select
            v-model="searchForm.downloadStatus"
            placeholder="全部"
            clearable
            style="width: 150px"
            @change="handleSearch"
          >
            <el-option label="待下载" :value="0" />
            <el-option label="下载中" :value="1" />
            <el-option label="下载完成" :value="2" />
            <el-option label="下载失败" :value="3" />
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

      <el-table
        v-loading="loading"
        :data="tableData"
        stripe
        style="width: 100%"
        @selection-change="handleSelectionChange"
      >
        <el-table-column type="selection" width="55" />
        <el-table-column prop="subscriptionName" label="订阅名称" width="150" />
        <el-table-column
          prop="rssMirrorItemTitle"
          label="RSS条目标题"
          min-width="200"
          show-overflow-tooltip
        />
        <el-table-column prop="rssSourceName" label="RSS源" width="120" />
        <el-table-column prop="rssMirrorItemLink" label="链接" width="100">
          <template #default="{ row }">
            <el-button
              link
              type="primary"
              @click="openLink(row.rssMirrorItemLink)"
            >
              打开
            </el-button>
          </template>
        </el-table-column>
        <el-table-column label="下载状态" width="120">
          <template #default="{ row }">
            <el-tag
              :type="
                getStatusType(
                  row.downloadStatus,
                  row.isPendingDueToLowDiskSpace
                )
              "
            >
              {{
                getStatusText(
                  row.downloadStatus,
                  row.isPendingDueToLowDiskSpace
                )
              }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column
          prop="aria2Gid"
          label="GID"
          width="120"
          show-overflow-tooltip
        />
        <el-table-column prop="downloadStartTime" label="开始时间" width="180">
          <template #default="{ row }">
            {{
              row.downloadStartTime ? formatDate(row.downloadStartTime) : "-"
            }}
          </template>
        </el-table-column>
        <el-table-column
          prop="downloadCompleteTime"
          label="完成时间"
          width="180"
        >
          <template #default="{ row }">
            {{
              row.downloadCompleteTime
                ? formatDate(row.downloadCompleteTime)
                : "-"
            }}
          </template>
        </el-table-column>
        <el-table-column prop="creationTime" label="创建时间" width="180">
          <template #default="{ row }">
            {{ formatDate(row.creationTime) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="150" fixed="right">
          <template #default="{ row }">
            <el-button
              v-if="row.downloadStatus === 3"
              size="small"
              type="primary"
              link
              @click="handleRetry(row)"
            >
              重试
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

      <div v-if="selectedRows.length > 0" class="batch-actions">
        <span class="selected-info">已选择 {{ selectedRows.length }} 项</span>
        <el-button type="danger" @click="handleBatchDelete">批量删除</el-button>
      </div>

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
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from "vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { rssSubscriptionDownloadApi } from "@/api/rssSubscriptionDownload";
import { rssSubscriptionApi } from "@/api/rssSubscription";
import type {
  RssSubscriptionDownloadDto,
  RssSubscriptionDto
} from "@/types/business";

const searchForm = reactive({
  subscriptionId: undefined as number | undefined,
  filter: "",
  downloadStatus: undefined as number | undefined,
  startTime: "",
  endTime: ""
});

const dateRange = ref<[string, string]>([]);

const pagination = reactive({
  page: 1,
  pageSize: 20,
  total: 0
});

const loading = ref(false);
const tableData = ref<RssSubscriptionDownloadDto[]>([]);
const selectedRows = ref<RssSubscriptionDownloadDto[]>([]);
const subscriptions = ref<RssSubscriptionDto[]>([]);

const fetchData = async () => {
  loading.value = true;
  try {
    const result = await rssSubscriptionDownloadApi.getList({
      pageIndex: pagination.page,
      pageSize: pagination.pageSize,
      sorting: "CreationTime desc",
      ...searchForm
    });
    tableData.value = result.items || [];
    pagination.total = result.totalCount || 0;
  } catch (error) {
    console.error("获取下载记录失败:", error);
    ElMessage.error("获取下载记录失败");
  } finally {
    loading.value = false;
  }
};

const fetchSubscriptions = async () => {
  try {
    const result = await rssSubscriptionApi.getList({
      pageIndex: 1,
      pageSize: 1000,
      isEnabled: true
    });
    subscriptions.value = result.items || [];
  } catch (error) {
    console.error("获取订阅列表失败:", error);
  }
};

const handleSearch = () => {
  pagination.page = 1;
  fetchData();
};

const handleReset = () => {
  searchForm.subscriptionId = undefined;
  searchForm.filter = "";
  searchForm.downloadStatus = undefined;
  searchForm.startTime = "";
  searchForm.endTime = "";
  dateRange.value = [];
  handleSearch();
};

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

const handleSelectionChange = (rows: RssSubscriptionDownloadDto[]) => {
  selectedRows.value = rows;
};

const handleDelete = async (row: RssSubscriptionDownloadDto) => {
  try {
    await ElMessageBox.confirm(
      `确定要删除下载记录 "${row.rssMirrorItemTitle}" 吗？`,
      "提示",
      {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning"
      }
    );

    await rssSubscriptionDownloadApi.delete(row.id);
    ElMessage.success("删除成功");
    fetchData();
  } catch (error) {
    if (error !== "cancel") {
      console.error("删除失败:", error);
      ElMessage.error("删除失败");
    }
  }
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
    await rssSubscriptionDownloadApi.deleteMany(ids);
    ElMessage.success("批量删除成功");
    fetchData();
  } catch (error) {
    if (error !== "cancel") {
      console.error("批量删除失败:", error);
      ElMessage.error("批量删除失败");
    }
  }
};

const handleClearAll = async () => {
  try {
    await ElMessageBox.confirm(
      "确定要清空所有下载记录吗？此操作不可恢复！",
      "警告",
      {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "error"
      }
    );

    await rssSubscriptionDownloadApi.clearAll();
    ElMessage.success("清空成功");
    fetchData();
  } catch (error) {
    if (error !== "cancel") {
      console.error("清空失败:", error);
      ElMessage.error("清空失败");
    }
  }
};

const handleRetry = async (row: RssSubscriptionDownloadDto) => {
  try {
    await rssSubscriptionDownloadApi.retry(row.id);
    ElMessage.success("已重新添加到下载队列");
    fetchData();
  } catch (error: any) {
    console.error("重试失败:", error);
    ElMessage.error(error.message || "重试失败");
  }
};

const openLink = (url: string) => {
  if (url) {
    window.open(url, "_blank");
  }
};

const getStatusText = (status: number, isPendingDueToLowDiskSpace: boolean) => {
  if (isPendingDueToLowDiskSpace) {
    return "空间暂存";
  }
  const statusMap: Record<number, string> = {
    0: "待下载",
    1: "下载中",
    2: "下载完成",
    3: "下载失败"
  };
  return statusMap[status] || "未知";
};

const getStatusType = (status: number, isPendingDueToLowDiskSpace: boolean) => {
  if (isPendingDueToLowDiskSpace) {
    return "warning";
  }
  const typeMap: Record<number, any> = {
    0: "info",
    1: "primary",
    2: "success",
    3: "danger"
  };
  return typeMap[status] || "info";
};

const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleString("zh-CN");
};

onMounted(() => {
  fetchSubscriptions();
  fetchData();
});
</script>

<style scoped>
.rss-subscription-downloads-container {
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

.batch-actions {
  display: flex;
  gap: 10px;
  align-items: center;
  padding: 10px;
  margin-top: 15px;
  background-color: #f5f7fa;
  border-radius: 4px;
}

.selected-info {
  font-weight: 500;
  color: #606266;
}
</style>
