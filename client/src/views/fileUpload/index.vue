<template>
  <div class="file-upload-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">文件上传管理</span>
          <div class="card-actions">
            <el-button type="primary" :icon="Upload" @click="handleUpload">
              上传文件
            </el-button>
            <el-button type="info" :icon="Refresh" @click="loadData">
              刷新列表
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
        @page-size-change="handleSizeChange"
        @page-current-change="handleCurrentChange"
      >
        <template #operation="{ row }">
          <el-button
            type="primary"
            size="small"
            :icon="Download"
            @click="handleDownload(row)"
          >
            下载
          </el-button>
          <el-button
            type="danger"
            size="small"
            :icon="Delete"
            @click="handleDelete(row)"
          >
            删除
          </el-button>
        </template>
      </pure-table>
    </el-card>

    <!-- 上传文件弹窗 -->
    <el-dialog
      v-model="uploadModalVisible"
      title="上传文件"
      width="600px"
      :close-on-click-modal="false"
    >
      <div class="upload-modal-content">
        <div
          class="custom-upload-area"
          :class="{ 'drag-over': isDragOver }"
          @click="triggerFileInput"
          @dragover.prevent="handleDragOver"
          @dragleave.prevent="handleDragLeave"
          @drop.prevent="handleDrop"
        >
          <input
            ref="fileInputRef"
            type="file"
            style="display: none"
            @change="handleFileInputChange"
          />
          <el-icon class="upload-icon"><upload-filled /></el-icon>
          <div class="upload-text">将文件拖到此处，或<em>点击选择文件</em></div>
          <div class="upload-tip">最大上传1MB的文件，支持计算文件SHA1值</div>
        </div>

        <!-- 已选择的文件 -->
        <div v-if="selectedFile" class="selected-file">
          <div class="file-info">
            <span class="file-name">{{ selectedFile.name }}</span>
            <span class="file-size">{{
              formatFileSize(selectedFile.size)
            }}</span>
          </div>
          <el-button type="danger" size="small" @click="clearSelectedFile">
            移除
          </el-button>
        </div>

        <!-- 上传进度 -->
        <div v-if="uploading" class="upload-progress">
          <el-progress :percentage="uploadProgress" :status="uploadStatus" />
          <p class="progress-text">正在上传: {{ currentFileName }}</p>
        </div>
      </div>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="uploadModalVisible = false">取消</el-button>
          <el-button type="primary" :loading="uploading" @click="submitUpload">
            开始上传
          </el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from "vue";
import { ElMessage, ElMessageBox } from "element-plus";
import {
  Upload,
  Refresh,
  Download,
  Delete,
  UploadFilled
} from "@element-plus/icons-vue";
import { fileUploadApi } from "@/api/fileUpload";
import type { FileUploadInfoDto, PagedRequestDto } from "@/types/business";
import dayjs from "dayjs";

// 表格数据
const loading = ref(false);
const tableData = ref<FileUploadInfoDto[]>([]);
const tableRef = ref();
const pagination = reactive({
  pageSize: 10,
  currentPage: 1,
  total: 0
});

// 表格列配置
const columns = ref([
  {
    type: "index",
    label: "#",
    width: 60
  },
  {
    label: "文件名",
    prop: "fileName",
    minWidth: 200
  },
  {
    label: "文件大小",
    prop: "fileSize",
    width: 120,
    formatter: (row: FileUploadInfoDto) => {
      return formatFileSize(row.fileSize);
    }
  },
  {
    label: "SHA1",
    prop: "sha1",
    minWidth: 300,
    showOverflowTooltip: true
  },
  {
    label: "路径",
    prop: "path",
    minWidth: 200,
    showOverflowTooltip: true
  },
  {
    label: "ID",
    prop: "id",
    width: 80
  },
  {
    label: "创建时间",
    prop: "creationTime",
    width: 180,
    formatter: (row: FileUploadInfoDto) => {
      return formatDateTime(row.creationTime);
    }
  },
  {
    label: "操作",
    slot: "operation",
    fixed: "right",
    width: 200
  }
]);

// 上传相关
const fileInputRef = ref<HTMLInputElement>();
const uploading = ref(false);
const uploadProgress = ref(0);
const uploadStatus = ref<"success" | "exception" | "warning" | "">("");
const currentFileName = ref("");
const isDragOver = ref(false);

// 上传弹窗
const uploadModalVisible = ref(false);
const selectedFile = ref<File | null>(null);

// 加载数据
const loadData = async () => {
  loading.value = true;
  try {
    const params: PagedRequestDto = {
      skipCount: (pagination.currentPage - 1) * pagination.pageSize,
      maxResultCount: pagination.pageSize
    };

    const result = await fileUploadApi.getFileUploadInfos(params);
    tableData.value = result.items || [];
    pagination.total = result.totalCount || 0;
  } catch (error) {
    console.error("加载数据失败:", error);
    ElMessage.error("加载数据失败");
  } finally {
    loading.value = false;
  }
};

// 格式化文件大小
const formatFileSize = (size: number): string => {
  if (size === 0) return "0 B";
  const k = 1024;
  const sizes = ["B", "KB", "MB", "GB"];
  const i = Math.floor(Math.log(size) / Math.log(k));
  return parseFloat((size / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
};

// 格式化日期时间
const formatDateTime = (dateTime?: string): string => {
  if (!dateTime) return "-";
  return dayjs(dateTime).format("YYYY-MM-DD HH:mm:ss");
};

// 处理分页大小变化
const handleSizeChange = (size: number) => {
  pagination.pageSize = size;
  pagination.currentPage = 1;
  loadData();
};

// 处理当前页变化
const handleCurrentChange = (page: number) => {
  pagination.currentPage = page;
  loadData();
};

// 上传相关方法
const handleUpload = () => {
  uploadModalVisible.value = true;
  // 重置上传状态
  uploading.value = false;
  uploadProgress.value = 0;
  uploadStatus.value = "";
  currentFileName.value = "";
  selectedFile.value = null;
  isDragOver.value = false;
};

const submitUpload = async () => {
  if (!selectedFile.value) {
    ElMessage.warning("请先选择文件");
    return;
  }

  // 检查文件大小
  if (selectedFile.value.size / 1024 / 1024 > 1) {
    ElMessage.error("文件大小不能超过1MB!");
    return;
  }

  uploading.value = true;
  uploadProgress.value = 0;
  uploadStatus.value = "";
  currentFileName.value = selectedFile.value.name;

  try {
    await fileUploadApi.uploadFile(selectedFile.value, (progress: number) => {
      uploadProgress.value = progress;
    });

    // 上传成功
    uploadStatus.value = "success";
    ElMessage.success("文件上传成功!");
    uploadModalVisible.value = false;
    loadData();
  } catch (error: any) {
    // 上传失败
    uploadStatus.value = "exception";
    console.error("上传失败:", error);
    ElMessage.error(`上传失败: ${error.message || "未知错误"}`);
  } finally {
    uploading.value = false;
    selectedFile.value = null;
  }
};

// 触发文件选择
const triggerFileInput = () => {
  fileInputRef.value?.click();
};

// 处理文件输入变化
const handleFileInputChange = (event: Event) => {
  const target = event.target as HTMLInputElement;
  if (target.files && target.files.length > 0) {
    selectedFile.value = target.files[0];
  }
};

// 清除选中的文件
const clearSelectedFile = () => {
  selectedFile.value = null;
  if (fileInputRef.value) {
    fileInputRef.value.value = "";
  }
};

// 拖拽相关方法
const handleDragOver = () => {
  isDragOver.value = true;
};

const handleDragLeave = () => {
  isDragOver.value = false;
};

const handleDrop = (event: DragEvent) => {
  isDragOver.value = false;
  if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
    selectedFile.value = event.dataTransfer.files[0];
  }
};

// 下载文件
const handleDownload = async (row: FileUploadInfoDto) => {
  try {
    const downloadUrl = await fileUploadApi.downloadFile(row.id);
    window.open(downloadUrl, "_blank");
  } catch (error) {
    console.error("下载失败:", error);
    ElMessage.error("下载失败");
  }
};

// 删除文件
const handleDelete = async (row: FileUploadInfoDto) => {
  try {
    await ElMessageBox.confirm(
      `确定要删除文件 "${row.fileName}" 吗？`,
      "确认删除",
      {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning"
      }
    );

    await fileUploadApi.deleteFileUploadInfo(row.id);
    ElMessage.success("删除成功");
    loadData();
  } catch (error) {
    if (error !== "cancel") {
      console.error("删除失败:", error);
      ElMessage.error("删除失败");
    }
  }
};

// 组件挂载时加载数据
onMounted(() => {
  loadData();
});
</script>

<style scoped>
.file-upload-container {
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
  gap: 8px;
}

.upload-section {
  padding: 20px;
  margin-bottom: 20px;
  background-color: #f5f7fa;
  border-radius: 4px;
}

.custom-upload-area {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px 20px;
  cursor: pointer;
  background-color: #fafafa;
  border: 2px dashed #d9d9d9;
  border-radius: 6px;
  transition: all 0.3s;
}

.custom-upload-area:hover {
  background-color: #f0f9ff;
  border-color: #409eff;
}

.custom-upload-area.drag-over {
  background-color: #ecf5ff;
  border-color: #409eff;
}

.upload-icon {
  margin-bottom: 16px;
  font-size: 48px;
  color: #c0c4cc;
}

.custom-upload-area:hover .upload-icon,
.custom-upload-area.drag-over .upload-icon {
  color: #409eff;
}

.upload-text {
  margin-bottom: 8px;
  font-size: 14px;
  color: #606266;
}

.upload-tip {
  font-size: 12px;
  color: #909399;
}

.selected-file {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px;
  margin-top: 16px;
  background-color: #f5f7fa;
  border-radius: 4px;
}

.file-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.file-name {
  font-weight: 500;
  color: #303133;
}

.file-size {
  font-size: 12px;
  color: #909399;
}

.upload-progress {
  padding: 15px;
  margin-top: 20px;
  background-color: #fff;
  border: 1px solid #e4e7ed;
  border-radius: 4px;
}

.progress-text {
  margin-top: 10px;
  font-size: 14px;
  color: #606266;
}

.table-container {
  margin-top: 20px;
}

.pagination-container {
  display: flex;
  justify-content: center;
  margin-top: 20px;
}

.ml-3 {
  margin-left: 12px;
}

.upload-modal-content {
  padding: 10px 0;
}

.dialog-footer {
  display: flex;
  gap: 8px;
  justify-content: flex-end;
}
</style>
