<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { downloadApi, type GlobalStatus } from '../api/downloader'
import { ElMessage } from 'element-plus'

const status = ref<GlobalStatus>({
  isConnected: false,
  activeDownloads: 0,
  pending: 0,
  downloading: 0,
  completed: 0,
  failed: 0,
  totalSpeedBytesPerSecond: 0,
  lastError: null,
  totalDownloadedBytes: 0,
  videoCount: 0,
})
const reconnecting = ref(false)
let timer: ReturnType<typeof setInterval> | null = null

const formatSpeed = (bytesPerSec: number) => {
  if (!bytesPerSec || bytesPerSec <= 0) return '0 B/s'
  if (bytesPerSec < 1024) return bytesPerSec.toFixed(0) + ' B/s'
  if (bytesPerSec < 1024 * 1024) return (bytesPerSec / 1024).toFixed(1) + ' KB/s'
  if (bytesPerSec < 1024 * 1024 * 1024) return (bytesPerSec / 1024 / 1024).toFixed(1) + ' MB/s'
  return (bytesPerSec / 1024 / 1024 / 1024).toFixed(2) + ' GB/s'
}

const formatBytes = (bytes: number) => {
  if (!bytes || bytes <= 0) return '0 B'
  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB', 'TB']
  const i = Math.min(Math.floor(Math.log(bytes) / Math.log(k)), sizes.length - 1)
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

const fetchStatus = async () => {
  try {
    const { data } = await downloadApi.getStatus()
    status.value = data
  } catch {
    // 静默处理
  }
}

const handleReconnect = async () => {
  reconnecting.value = true
  try {
    const { data } = await downloadApi.reconnect()
    status.value.isConnected = data.isConnected
    status.value.lastError = data.lastError
    if (data.isConnected) {
      ElMessage.success('已连接 DFApp 后端')
    } else {
      ElMessage.warning(data.lastError || '连接失败')
    }
  } catch {
    ElMessage.error('重连请求失败')
  } finally {
    reconnecting.value = false
  }
}

onMounted(() => {
  fetchStatus()
  timer = setInterval(fetchStatus, 3000)
})

onUnmounted(() => {
  if (timer) clearInterval(timer)
})
</script>

<template>
  <div>
    <h2 style="margin-bottom: 20px">仪表盘</h2>

    <el-row :gutter="20">
      <el-col :span="6">
        <el-card shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><Connection /></el-icon>
              <span>连接状态</span>
              <el-button
                style="margin-left: auto"
                size="small"
                :loading="reconnecting"
                @click="handleReconnect"
              >
                重新连接
              </el-button>
            </div>
          </template>
          <div class="status-value">
            <el-tag :type="status.isConnected ? 'success' : 'danger'" size="large">
              {{ status.isConnected ? '已连接' : '未连接' }}
            </el-tag>
          </div>
          <el-alert
            v-if="!status.isConnected && status.lastError"
            :title="status.lastError"
            type="error"
            :closable="false"
            show-icon
            style="margin-top: 10px; text-align: left"
          />
        </el-card>
      </el-col>

      <el-col :span="6">
        <el-card shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><Loading /></el-icon>
              <span>活跃下载</span>
            </div>
          </template>
          <div class="status-value number">{{ status.activeDownloads }}</div>
        </el-card>
      </el-col>

      <el-col :span="6">
        <el-card shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><Clock /></el-icon>
              <span>等待队列</span>
            </div>
          </template>
          <div class="status-value number">{{ status.pending }}</div>
        </el-card>
      </el-col>

      <el-col :span="6">
        <el-card shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><CircleCheck /></el-icon>
              <span>已完成</span>
            </div>
          </template>
          <div class="status-value number success">{{ status.completed }}</div>
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="20" style="margin-top: 20px">
      <el-col :span="6">
        <el-card shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><Download /></el-icon>
              <span>下载中</span>
            </div>
          </template>
          <div class="status-value number primary">{{ status.downloading }}</div>
        </el-card>
      </el-col>

      <el-col :span="6">
        <el-card shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><CircleClose /></el-icon>
              <span>失败</span>
            </div>
          </template>
          <div class="status-value number danger">{{ status.failed }}</div>
        </el-card>
      </el-col>

      <el-col :span="6">
        <el-card shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><DataLine /></el-icon>
              <span>下载速度</span>
            </div>
          </template>
          <div class="status-value number primary">{{ formatSpeed(status.totalSpeedBytesPerSecond) }}</div>
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="20" style="margin-top: 20px">
      <el-col :span="8">
        <el-card shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><Coin /></el-icon>
              <span>累计下载</span>
            </div>
          </template>
          <div class="status-value number">{{ formatBytes(status.totalDownloadedBytes) }}</div>
        </el-card>
      </el-col>

      <el-col :span="8">
        <el-card shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><VideoCamera /></el-icon>
              <span>视频</span>
            </div>
          </template>
          <div class="status-value number">{{ status.videoCount }} 个</div>
        </el-card>
      </el-col>

      <el-col :span="8">
        <el-card shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><Files /></el-icon>
              <span>文件</span>
            </div>
          </template>
          <div class="status-value number">{{ status.completed }} 个</div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<style scoped>
.card-header {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
}

.status-value {
  text-align: center;
  padding: 10px 0;
}

.status-value.number {
  font-size: 36px;
  font-weight: bold;
  color: #303133;
}

.status-value.number.success {
  color: #67c23a;
}

.status-value.number.primary {
  color: #409eff;
}

.status-value.number.danger {
  color: #f56c6c;
}
</style>
