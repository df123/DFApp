<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { downloadApi, type GlobalStatus } from '../api/downloader'

const status = ref<GlobalStatus>({
  isConnected: false,
  activeDownloads: 0,
  pending: 0,
  downloading: 0,
  completed: 0,
  failed: 0,
})
let timer: ReturnType<typeof setInterval> | null = null

const fetchStatus = async () => {
  try {
    const { data } = await downloadApi.getStatus()
    status.value = data
  } catch {
    // 静默处理
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
            </div>
          </template>
          <div class="status-value">
            <el-tag :type="status.isConnected ? 'success' : 'danger'" size="large">
              {{ status.isConnected ? '已连接' : '未连接' }}
            </el-tag>
          </div>
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
