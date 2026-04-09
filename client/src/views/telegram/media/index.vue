<template>
  <div class="media-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">媒体管理</span>
          <div class="card-actions">
            <el-input
              v-model="searchKeyword"
              placeholder="搜索媒体ID、聊天标题或消息"
              style="width: 300px; margin-right: 10px"
              clearable
              @clear="handleSearch"
              @keyup.enter="handleSearch"
            >
              <template #prefix>
                <el-icon><Search /></el-icon>
              </template>
            </el-input>
            <el-button type="primary" @click="handleSearch">搜索</el-button>
            <el-button type="danger" @click="handleDeleteInvalidItems">
              删除无效项目
            </el-button>
          </div>
        </div>
      </template>

      <!-- 数据表格 -->
      <pure-table
        ref="tableRef"
        :loading="loading"
        :data="mediaList"
        :columns="columns"
        :pagination="pagination"
        @page-size-change="handleSizeChange"
        @page-current-change="handleCurrentChange"
        @selection-change="handleSelectionChange"
      />
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, h } from "vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { Search } from "@element-plus/icons-vue";
import { mediaInfoApi } from "@/api/mediaInfo";
import type { MediaInfoDto } from "@/types/business";
import { hasPerms } from "@/utils/auth";
import { formatFileSize } from "@/utils/format";

// 权限检查函数
const hasPermission = (permission: string): boolean => {
  return hasPerms(permission);
};

// 加载状态
const loading = ref(false);

// 表格引用
const tableRef = ref();

// 媒体列表数据
const mediaList = ref<MediaInfoDto[]>([]);
const selectedMedia = ref<MediaInfoDto[]>([]);

// 搜索关键字
const searchKeyword = ref("");

// 分页配置
const pagination = reactive({
  currentPage: 1,
  pageSize: 10,
  total: 0
});

// 表格列配置
const columns = ref([
  {
    label: "媒体ID",
    prop: "mediaId",
    width: 180
  },
  {
    label: "是否下载完成",
    width: 120,
    formatter: (row: MediaInfoDto) => {
      return row.isDownloadCompleted ? "是" : "否";
    }
  },
  {
    label: "是否生成外部链接",
    width: 140,
    formatter: (row: MediaInfoDto) => {
      return row.isExternalLinkGenerated ? "是" : "否";
    }
  },
  {
    label: "媒体大小",
    prop: "size",
    width: 120,
    formatter: (row: MediaInfoDto) => {
      return formatFileSize(row.size || 0);
    }
  },
  {
    label: "下载时间",
    prop: "downloadTimeMs",
    width: 120,
    formatter: (row: MediaInfoDto) => {
      if (!row.downloadTimeMs) return "-";
      return `${(row.downloadTimeMs / 1000).toFixed(2)} s`;
    }
  },
  {
    label: "下载速度",
    prop: "downloadSpeedBps",
    width: 120,
    formatter: (row: MediaInfoDto) => {
      if (!row.downloadSpeedBps) return "-";
      return formatFileSize(row.downloadSpeedBps) + "/s";
    }
  },
  {
    label: "媒体类型",
    prop: "mimeType",
    width: 120
  },
  {
    label: "创建时间",
    prop: "creationTime",
    width: 180,
    formatter: (row: MediaInfoDto) => {
      return formatDateTime(row.creationTime);
    }
  },
  {
    label: "聊天ID",
    prop: "chatId",
    width: 120
  },
  {
    label: "聊天标题",
    prop: "chatTitle",
    width: 150
  },
  {
    label: "消息",
    prop: "message",
    minWidth: 200,
    showOverflowTooltip: true
  }
]);

// 格式化日期时间
const formatDateTime = (dateString: string): string => {
  if (!dateString) return "";
  const date = new Date(dateString);
  return date.toLocaleString();
};

// 获取媒体列表
const getMediaList = async () => {
  loading.value = true;
  try {
    const params = {
      skipCount: (pagination.currentPage - 1) * pagination.pageSize,
      maxResultCount: pagination.pageSize,
      filter: searchKeyword.value || undefined
    };
    const result = await mediaInfoApi.getList(params);
    mediaList.value = result.items || [];
    pagination.total = result.totalCount || 0;
  } catch (error) {
    console.error("获取媒体列表失败:", error);
    ElMessage.error("获取媒体列表失败");
  } finally {
    loading.value = false;
  }
};

// 处理搜索
const handleSearch = () => {
  pagination.currentPage = 1;
  getMediaList();
};

// 处理分页大小变化
const handleSizeChange = (size: number) => {
  pagination.pageSize = size;
  pagination.currentPage = 1;
  getMediaList();
};

// 处理当前页变化
const handleCurrentChange = (page: number) => {
  pagination.currentPage = page;
  getMediaList();
};

// 处理表格选择变化
const handleSelectionChange = (selection: MediaInfoDto[]) => {
  selectedMedia.value = selection;
};

// 处理删除无效项目
const handleDeleteInvalidItems = async () => {
  try {
    await ElMessageBox.confirm("确定要删除所有无效项目吗？", "删除确认", {
      confirmButtonText: "确定",
      cancelButtonText: "取消",
      type: "warning"
    });

    await mediaInfoApi.deleteInvalidItems();
    ElMessage.success("无效项目删除成功");
    await getMediaList();
  } catch (error) {
    if (error !== "cancel") {
      console.error("删除无效项目失败:", error);
      ElMessage.error("删除无效项目失败");
    }
  }
};

// 组件挂载时获取数据
onMounted(() => {
  getMediaList();
});
</script>

<style scoped>
.media-container {
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

.card-actions {
  display: flex;
  gap: 10px;
}
</style>
