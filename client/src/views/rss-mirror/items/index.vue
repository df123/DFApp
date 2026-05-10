<template>
  <div class="rss-mirror-items-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">RSS镜像条目</span>
          <div>
            <el-button type="danger" @click="handleClearAll"
              >清空所有</el-button
            >
          </div>
        </div>
      </template>

      <!-- 搜索表单 -->
      <el-form :inline="true" :model="searchForm" class="search-form">
        <el-form-item label="RSS源">
          <el-select
            v-model="searchForm.rssSourceId"
            placeholder="全部"
            clearable
            style="width: 300px"
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
        <el-form-item label="关键词">
          <el-input
            v-model="searchForm.filter"
            placeholder="搜索标题或描述"
            clearable
            @clear="handleSearch"
          />
        </el-form-item>
        <el-form-item label="下载状态">
          <el-select
            v-model="searchForm.isDownloaded"
            placeholder="全部"
            clearable
            @change="handleSearch"
          >
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

      <!-- 表格 -->
      <el-table
        v-loading="loading"
        :data="tableData"
        stripe
        style="width: 100%"
        @selection-change="handleSelectionChange"
      >
        <el-table-column type="selection" width="55" />
        <el-table-column
          prop="title"
          label="标题"
          min-width="200"
          show-overflow-tooltip
        />
        <el-table-column prop="rssSourceName" label="RSS源" width="150" />
        <el-table-column prop="link" label="链接" width="100">
          <template #default="{ row }">
            <el-button link type="primary" @click="openLink(row.link)">
              打开
            </el-button>
          </template>
        </el-table-column>
        <el-table-column prop="publishDate" label="发布时间" width="180">
          <template #default="{ row }">
            {{ row.publishDate ? formatDate(row.publishDate) : "-" }}
          </template>
        </el-table-column>
        <el-table-column prop="seeders" label="做种" width="80">
          <template #default="{ row }">
            <el-tag
              v-if="row.seeders !== null && row.seeders !== undefined"
              type="success"
            >
              {{ row.seeders }}
            </el-tag>
            <span v-else>-</span>
          </template>
        </el-table-column>
        <el-table-column prop="leechers" label="下载" width="80">
          <template #default="{ row }">
            <el-tag
              v-if="row.leechers !== null && row.leechers !== undefined"
              type="warning"
            >
              {{ row.leechers }}
            </el-tag>
            <span v-else>-</span>
          </template>
        </el-table-column>
        <el-table-column label="下载状态" width="100">
          <template #default="{ row }">
            <el-tag :type="row.isDownloaded ? 'success' : 'info'">
              {{ row.isDownloaded ? "已下载" : "未下载" }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="creationTime" label="抓取时间" width="180">
          <template #default="{ row }">
            {{ formatDate(row.creationTime) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="200" fixed="right">
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

      <!-- 批量操作 -->
      <div v-if="selectedRows.length > 0" class="batch-actions">
        <span class="selected-info">已选择 {{ selectedRows.length }} 项</span>
        <el-button type="danger" @click="handleBatchDelete">批量删除</el-button>
      </div>

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

    <!-- 下载选项对话框 -->
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
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from "vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { rssMirrorApi } from "@/api/rssMirror";
import { rssSourceApi } from "@/api/rssSource";
import type {
  RssMirrorItemDto,
  RssSourceDto
} from "@/types/business";

// 搜索表单
const searchForm = reactive({
  rssSourceId: undefined as number | undefined,
  filter: "",
  isDownloaded: undefined as boolean | undefined,
  startTime: "",
  endTime: ""
});

const dateRange = ref<[string, string]>([]);

// 分页
const pagination = reactive({
  page: 1,
  pageSize: 10,
  total: 0
});

// 表格数据
const loading = ref(false);
const tableData = ref<RssMirrorItemDto[]>([]);
const selectedRows = ref<RssMirrorItemDto[]>([]);

// RSS源列表
const rssSources = ref<RssSourceDto[]>([]);

// 下载选项对话框
const showDownloadDialog = ref(false);
const downloadOptions = reactive({
  videoOnly: true,
  enableKeywordFilter: true
});
let currentDownloadItem: RssMirrorItemDto | null = null;

// 获取数据
const fetchData = async () => {
  loading.value = true;
  try {
    const result = await rssMirrorApi.getList({
      pageIndex: pagination.page,
      pageSize: pagination.pageSize,
      sorting: "CreationTime desc",
      ...searchForm
    });
    tableData.value = result.items || [];
    pagination.total = result.totalCount || 0;
  } catch (error) {
    console.error("获取RSS镜像条目失败:", error);
    ElMessage.error("获取RSS镜像条目失败");
  } finally {
    loading.value = false;
  }
};

// 获取RSS源列表
const fetchRssSources = async () => {
  try {
    const result = await rssSourceApi.getList({
      pageIndex: 1,
      pageSize: 1000
    });
    rssSources.value = result.items || [];
  } catch (error) {
    console.error("获取RSS源列表失败:", error);
  }
};

// 搜索
const handleSearch = () => {
  pagination.page = 1;
  fetchData();
};

// 重置
const handleReset = () => {
  searchForm.rssSourceId = undefined;
  searchForm.filter = "";
  searchForm.isDownloaded = undefined;
  searchForm.startTime = "";
  searchForm.endTime = "";
  dateRange.value = [];
  handleSearch();
};

// 日期范围变化
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

// 选择变化
const handleSelectionChange = (rows: RssMirrorItemDto[]) => {
  selectedRows.value = rows;
};

// 删除
const handleDelete = async (row: RssMirrorItemDto) => {
  try {
    await ElMessageBox.confirm(`确定要删除条目 "${row.title}" 吗？`, "提示", {
      confirmButtonText: "确定",
      cancelButtonText: "取消",
      type: "warning"
    });

    await rssMirrorApi.delete(row.id);
    ElMessage.success("删除成功");
    fetchData();
  } catch (error) {
    if (error !== "cancel") {
      console.error("删除失败:", error);
      ElMessage.error("删除失败");
    }
  }
};

// 批量删除
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

// 清空所有
const handleClearAll = async () => {
  try {
    await ElMessageBox.confirm(
      "确定要清空所有RSS镜像数据吗？此操作不可恢复！",
      "警告",
      {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "error"
      }
    );

    await rssMirrorApi.clearAll();
    ElMessage.success("清空成功");
    fetchData();
  } catch (error) {
    if (error !== "cancel") {
      console.error("清空失败:", error);
      ElMessage.error("清空失败");
    }
  }
};

// 下载
const handleDownload = (row: RssMirrorItemDto) => {
  currentDownloadItem = row;
  showDownloadDialog.value = true;
};

// 确认下载
const confirmDownload = async () => {
  if (!currentDownloadItem) return;

  try {
    await rssMirrorApi.downloadToAria2(
      currentDownloadItem.id,
      downloadOptions.videoOnly,
      downloadOptions.enableKeywordFilter
    );
    ElMessage.success("已添加到下载队列");
    showDownloadDialog.value = false;
    fetchData();
  } catch (error: any) {
    console.error("下载失败:", error);
    ElMessage.error(error.message || "下载失败");
  }
};

// 打开链接
const openLink = (url: string) => {
  window.open(url, "_blank");
};

// 格式化日期
const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleString("zh-CN");
};

// 生命周期
onMounted(() => {
  fetchRssSources();
  fetchData();
});
</script>

<style scoped>
.rss-mirror-items-container {
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

.form-tip {
  margin-top: 5px;
  font-size: 12px;
  color: #909399;
}
</style>
