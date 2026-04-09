<template>
  <div class="aria2-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">Aria2管理</span>
          <div class="card-actions">
            <el-button
              v-permission="['DFApp.Aria2.Link']"
              type="primary"
              @click="handleGetAllLinks"
            >
              <el-icon><Link /></el-icon>
              获取全部链接
            </el-button>
            <div v-permission="['DFApp.Aria2.Link']" class="video-only-switch">
              <el-switch v-model="videoOnly" active-text="只获取视频文件" />
            </div>
            <el-button
              v-permission="['DFApp.Aria2.Delete']"
              type="danger"
              @click="handleDeleteAll"
            >
              <el-icon><Delete /></el-icon>
              删除全部
            </el-button>
            <el-button
              v-permission="['DFApp.Aria2.Delete']"
              type="warning"
              @click="handleClearDownloadDirectory"
            >
              <el-icon><Delete /></el-icon>
              清空下载目录
            </el-button>
          </div>
        </div>
      </template>

      <!-- 数据表格 -->
      <pure-table
        ref="tableRef"
        :loading="loading"
        :data="tableData"
        :columns="columns"
        :pagination="pagination"
        :page-sizes="[10, 20, 50, 100]"
        @page-size-change="handleSizeChange"
        @page-current-change="handleCurrentChange"
      >
        <template #operation="{ row }">
          <el-button
            v-permission="['DFApp.Aria2.Link']"
            size="small"
            type="primary"
            link
            @click="handleGetLink(row)"
          >
            链接
          </el-button>
          <el-button
            v-permission="['DFApp.Aria2.Delete']"
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

    <!-- 链接内容对话框 -->
    <el-dialog
      v-model="linkDialogVisible"
      title="链接地址"
      width="600px"
      :before-close="handleCloseLinkDialog"
    >
      <el-form label-width="100px">
        <el-form-item label="链接地址">
          <el-input v-model="linkContent" type="textarea" :rows="6" readonly />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button type="primary" @click="handleCopyLink">
          <el-icon><DocumentCopy /></el-icon>
          复制链接
        </el-button>
        <el-button @click="handleCloseLinkDialog">关闭</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from "vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { Link, Delete, DocumentCopy } from "@element-plus/icons-vue";
import { useAria2Api } from "@/api/aria2";
import type { TellStatusResultDto } from "@/types/business";

// API 服务
const aria2Api = useAria2Api();

// 响应式数据
const loading = ref(false);
const tableRef = ref();
const linkDialogVisible = ref(false);
const linkContent = ref("");
const videoOnly = ref(true);

// 分页数据
const pagination = reactive({
  pageSize: 10,
  currentPage: 1,
  total: 0
});

// 表格数据
const tableData = ref<TellStatusResultDto[]>([]);

// 表格列配置
const columns = ref([
  {
    label: "ID",
    prop: "id",
    width: 80
  },
  {
    label: "GID",
    prop: "gid",
    minWidth: 120,
    showOverflowTooltip: true
  },
  {
    label: "状态",
    prop: "status",
    width: 100,
    formatter: (row: TellStatusResultDto) => {
      return getStatusText(row.status);
    }
  },
  {
    label: "大小",
    prop: "totalLength",
    width: 120,
    formatter: (row: TellStatusResultDto) => {
      return convertBytes(row.totalLength);
    }
  },
  {
    label: "文件",
    prop: "files",
    minWidth: 200,
    showOverflowTooltip: true,
    formatter: (row: TellStatusResultDto) => {
      return row.files && row.files.length > 0 ? row.files[0].path : "-";
    }
  },
  {
    label: "创建时间",
    prop: "creationTime",
    width: 180,
    formatter: (row: TellStatusResultDto) => {
      return row.creationTime
        ? new Date(row.creationTime).toLocaleString()
        : "-";
    }
  },
  {
    label: "操作",
    slot: "operation",
    fixed: "right",
    width: 150
  }
]);

// 方法
const loadTableData = async () => {
  loading.value = true;
  try {
    const params = {
      skipCount: (pagination.currentPage - 1) * pagination.pageSize,
      maxResultCount: pagination.pageSize
    };

    const result = await aria2Api.getAria2Status(params);
    tableData.value = result.items;
    pagination.total = result.totalCount;
  } catch (error) {
    console.error("加载Aria2数据失败:", error);
    ElMessage.error("加载Aria2数据失败");
  } finally {
    loading.value = false;
  }
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

const handleGetLink = async (row: TellStatusResultDto) => {
  try {
    const link = await aria2Api.getExternalLink(row.id);
    linkContent.value = link;
    linkDialogVisible.value = true;
  } catch (error) {
    console.error("获取链接失败:", error);
    ElMessage.error("获取链接失败");
  }
};

const handleGetAllLinks = async () => {
  try {
    const links = await aria2Api.getAllExternalLinks(videoOnly.value);
    linkContent.value = links.join("\n");
    linkDialogVisible.value = true;
  } catch (error) {
    console.error("获取全部链接失败:", error);
    ElMessage.error("获取全部链接失败");
  }
};

const handleCopyLink = async () => {
  try {
    await navigator.clipboard.writeText(linkContent.value);
    ElMessage.success("复制成功！");
  } catch (error) {
    console.error("复制链接失败:", error);
    ElMessage.error("复制链接失败");
  }
};

const handleCloseLinkDialog = () => {
  linkDialogVisible.value = false;
  linkContent.value = "";
};

const handleDelete = async (row: TellStatusResultDto) => {
  try {
    await ElMessageBox.confirm(`您确定要删除吗 '${row.id}'?`, "删除确认", {
      confirmButtonText: "确定",
      cancelButtonText: "取消",
      type: "warning"
    });

    await aria2Api.delete(row.id);
    ElMessage.success("删除成功！");
    loadTableData();
  } catch (error) {
    if (error !== "cancel") {
      console.error("删除失败:", error);
      ElMessage.error("删除失败");
    }
  }
};

const handleDeleteAll = async () => {
  try {
    await ElMessageBox.confirm(
      "Are you sure you want to proceed?",
      "删除全部确认",
      {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning"
      }
    );

    await aria2Api.deleteAll();
    ElMessage.success("删除全部成功！");
    loadTableData();
  } catch (error) {
    if (error !== "cancel") {
      console.error("删除全部失败:", error);
      ElMessage.error("删除全部失败");
    }
  }
};

const handleClearDownloadDirectory = async () => {
  try {
    await ElMessageBox.confirm(
      "确定要清空下载目录吗？此操作将删除目录下的所有文件和子文件夹，且不可恢复。",
      "清空下载目录确认",
      {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning"
      }
    );

    await aria2Api.clearDownloadDirectory();
    ElMessage.success("清空下载目录成功！");
    loadTableData();
  } catch (error) {
    if (error !== "cancel") {
      console.error("清空下载目录失败:", error);
      ElMessage.error("清空下载目录失败");
    }
  }
};

// 工具函数：转换字节大小为可读格式
const convertBytes = (bytes: string): string => {
  const bytesNum = parseInt(bytes);
  if (isNaN(bytesNum)) return "-";

  const sizes = ["Bytes", "KB", "MB", "GB", "TB"];
  if (bytesNum === 0) return "0 Bytes";

  const i = Math.floor(Math.log(bytesNum) / Math.log(1024));
  return (
    Math.round((bytesNum / Math.pow(1024, i)) * 100) / 100 + " " + sizes[i]
  );
};

// 工具函数：获取状态文本
const getStatusText = (status: string): string => {
  const statusMap: Record<string, string> = {
    active: "活跃",
    waiting: "等待中",
    paused: "已暂停",
    error: "错误",
    complete: "已完成",
    removed: "已移除"
  };
  return statusMap[status] || status;
};

// 生命周期
onMounted(() => {
  loadTableData();
});
</script>

<style scoped>
.aria2-container {
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
  align-items: center;
}

.video-only-switch {
  margin-left: 5px;
}
</style>
