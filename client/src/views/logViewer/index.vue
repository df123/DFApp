<template>
  <div class="log-viewer-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">日志查看器</span>
        </div>
      </template>

      <div class="log-viewer-content">
        <el-row :gutter="20">
          <!-- 左侧日志文件列表 -->
          <el-col :span="6">
            <el-card class="file-list-card">
              <template #header>
                <div class="file-list-header">
                  <span>日志文件</span>
                </div>
              </template>

              <div v-loading="loadingFiles" class="file-list">
                <div
                  v-if="!logFiles.length && !loadingFiles"
                  class="empty-files"
                >
                  <el-empty description="没有找到日志文件" />
                </div>

                <div v-else>
                  <div
                    v-for="file in logFiles"
                    :key="file.name"
                    class="file-item"
                    :class="{ active: currentFile === file.name }"
                    @click="selectFile(file.name)"
                  >
                    <div class="file-info">
                      <div class="file-name">{{ file.name }}</div>
                      <div class="file-meta">
                        <span class="file-size">{{
                          formatFileSize(file.size)
                        }}</span>
                        <span class="file-date">{{
                          formatDate(file.lastModified)
                        }}</span>
                      </div>
                    </div>
                    <div class="file-actions" @click.stop>
                      <el-button
                        type="text"
                        size="small"
                        title="下载"
                        @click="downloadFile(file.name)"
                      >
                        <el-icon><Download /></el-icon>
                      </el-button>
                    </div>
                  </div>
                </div>
              </div>
            </el-card>
          </el-col>

          <!-- 右侧日志内容 -->
          <el-col :span="18">
            <el-card class="log-content-card">
              <template #header>
                <div class="log-content-header">
                  <span class="current-file-name">{{
                    currentFile || "选择日志文件"
                  }}</span>
                  <div class="log-controls">
                    <el-switch
                      v-model="autoRefresh"
                      active-text="实时更新"
                      @change="toggleAutoRefresh"
                    />
                    <el-button
                      type="primary"
                      size="small"
                      :icon="Refresh"
                      :disabled="!currentFile"
                      @click="refreshContent"
                    >
                      刷新
                    </el-button>
                  </div>
                </div>
              </template>

              <div v-loading="loadingContent" class="log-content">
                <pre v-if="logContent" ref="logContentRef" class="log-text">{{
                  logContent
                }}</pre>
                <div v-else-if="!loadingContent" class="empty-content">
                  <el-empty description="请选择日志文件查看内容" />
                </div>
              </div>
            </el-card>
          </el-col>
        </el-row>
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, nextTick } from "vue";
import { ElMessage } from "element-plus";
import { Download, Refresh } from "@element-plus/icons-vue";
import { useLogViewerApi } from "@/api/logViewer";
import type { LogFileInfoDto } from "@/types/business";

// API 实例
const logViewerApi = useLogViewerApi();

// 响应式数据
const logFiles = ref<LogFileInfoDto[]>([]);
const currentFile = ref("");
const logContent = ref("");
const loadingFiles = ref(false);
const loadingContent = ref(false);
const autoRefresh = ref(false);
const autoRefreshInterval = ref<number | null>(null);
const logContentRef = ref<HTMLElement | null>(null);

// 加载日志文件列表
const loadLogFiles = async () => {
  loadingFiles.value = true;
  try {
    logFiles.value = await logViewerApi.getLogFiles();
  } catch (error) {
    console.error("加载日志文件列表失败:", error);
    ElMessage.error("加载日志文件列表失败");
  } finally {
    loadingFiles.value = false;
  }
};

// 加载日志内容
const loadLogContent = async (fileName: string, isTail = true) => {
  if (!fileName) {
    console.error("文件名不能为空");
    return;
  }

  loadingContent.value = true;
  try {
    logContent.value = await logViewerApi.getLogContent(fileName, isTail);
    if (logContent.value) {
      await nextTick();
      scrollToBottom();
    }
  } catch (error: any) {
    console.error("加载日志失败:", error);
    let errorMessage = "加载日志内容失败";
    if (error.response?.status === 404) {
      errorMessage = "找不到日志文件";
    } else if (error.response?.data) {
      errorMessage = error.response.data.message || error.response.data;
    }
    ElMessage.error(errorMessage);
  } finally {
    loadingContent.value = false;
  }
};

// 选择文件
const selectFile = (fileName: string) => {
  currentFile.value = fileName;
  loadLogContent(fileName);
};

// 下载文件
const downloadFile = (fileName: string) => {
  logViewerApi.downloadLogFile(fileName);
};

// 刷新内容
const refreshContent = () => {
  if (currentFile.value) {
    loadLogContent(currentFile.value);
  }
};

// 切换自动刷新
const toggleAutoRefresh = (enable: boolean) => {
  if (enable && currentFile.value) {
    autoRefreshInterval.value = window.setInterval(() => {
      loadLogContent(currentFile.value);
    }, 2000);
  } else {
    if (autoRefreshInterval.value) {
      clearInterval(autoRefreshInterval.value);
      autoRefreshInterval.value = null;
    }
  }
};

// 滚动到底部
const scrollToBottom = () => {
  if (logContentRef.value) {
    logContentRef.value.scrollTop = logContentRef.value.scrollHeight;
  }
};

// 格式化文件大小
const formatFileSize = (bytes: number): string => {
  if (bytes === 0) return "0 B";
  const k = 1024;
  const sizes = ["B", "KB", "MB", "GB"];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
};

// 格式化日期
const formatDate = (dateString: string): string => {
  return new Date(dateString).toLocaleString();
};

// 生命周期
onMounted(() => {
  loadLogFiles();
});

onUnmounted(() => {
  if (autoRefreshInterval.value) {
    clearInterval(autoRefreshInterval.value);
  }
});
</script>

<style scoped>
.log-viewer-container {
  box-sizing: border-box;
  display: flex;
  flex-direction: column;
  height: 80vh;
  padding: 12px;
}

.log-viewer-container :deep(.el-card) {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.log-viewer-container :deep(.el-card__body) {
  display: flex;
  flex: 1;
  flex-direction: column;
  padding: 12px;
  overflow: hidden;
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

.log-viewer-content {
  display: flex;
  flex: 1;
  flex-direction: column;
  overflow: hidden;
}

.log-viewer-content :deep(.el-row) {
  height: 100%;
}

.log-viewer-content :deep(.el-col) {
  height: 100%;
}

.file-list-card {
  height: 100%;
}

.file-list-card :deep(.el-card__body) {
  display: flex;
  flex-direction: column;
  height: calc(100% - 57px);
  padding: 0;
}

.file-list-header {
  font-weight: 600;
}

.file-list {
  flex: 1;
  overflow-y: auto;
}

.empty-files {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100%;
}

.file-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 12px;
  cursor: pointer;
  border-bottom: 1px solid #f0f0f0;
  border-radius: 0;
  transition: background-color 0.2s;
}

.file-item:hover {
  background-color: #f5f7fa;
}

.file-item.active {
  background-color: #e6f7ff;
  border-color: #1890ff;
}

.file-info {
  flex: 1;
  min-width: 0;
}

.file-name {
  margin-bottom: 4px;
  font-size: 13px;
  font-weight: 500;
  word-break: break-all;
}

.file-meta {
  display: flex;
  justify-content: space-between;
  font-size: 11px;
  color: #999;
}

.file-actions {
  margin-left: 8px;
}

.log-content-card {
  height: 100%;
}

.log-content-card :deep(.el-card__body) {
  display: flex;
  flex-direction: column;
  height: calc(100% - 57px);
  padding: 0;
}

.log-content-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px;
  border-bottom: 1px solid #f0f0f0;
}

.current-file-name {
  font-size: 14px;
  font-weight: 600;
}

.log-controls {
  display: flex;
  gap: 12px;
  align-items: center;
}

.log-content {
  flex: 1;
  padding: 12px;
  overflow: hidden;
}

.log-text {
  height: 100%;
  padding: 12px;
  margin: 0;
  overflow-y: auto;
  font-family: "Courier New", monospace;
  font-size: 12px;
  line-height: 1.4;
  word-break: break-all;
  white-space: pre-wrap;
  background-color: #f8f9fa;
  border-radius: 4px;
}

.empty-content {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100%;
}
</style>
