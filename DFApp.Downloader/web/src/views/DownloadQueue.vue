<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { downloadApi, type DownloadItem } from '../api/downloader'
import { ElMessage, ElMessageBox } from 'element-plus'

const downloads = ref<DownloadItem[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)
const statusFilter = ref('')
const loading = ref(false)
let timer: ReturnType<typeof setInterval> | null = null

const statusMap: Record<string, { label: string; type: string }> = {
  Pending: { label: '等待中', type: 'info' },
  Downloading: { label: '下载中', type: '' },
  Paused: { label: '已暂停', type: 'warning' },
  Completed: { label: '已完成', type: 'success' },
  Failed: { label: '失败', type: 'danger' },
}

const fetchDownloads = async () => {
  loading.value = true
  try {
    const { data } = await downloadApi.getList(page.value, pageSize.value, statusFilter.value || undefined)
    downloads.value = data.items
    total.value = data.total
  } catch {
    ElMessage.error('获取下载列表失败')
  } finally {
    loading.value = false
  }
}

const handlePause = async (id: number) => {
  try {
    await downloadApi.pause(id)
    ElMessage.success('已暂停')
    fetchDownloads()
  } catch {
    ElMessage.error('暂停失败')
  }
}

const handleResume = async (id: number) => {
  try {
    await downloadApi.resume(id)
    ElMessage.success('已恢复')
    fetchDownloads()
  } catch {
    ElMessage.error('恢复失败')
  }
}

const handleRetry = async (id: number) => {
  try {
    await downloadApi.resume(id)
    ElMessage.success('已重新加入队列')
    fetchDownloads()
  } catch {
    ElMessage.error('重试失败')
  }
}

const handleCancel = async (id: number) => {
  try {
    await ElMessageBox.confirm('确定要取消并删除此下载任务吗？', '确认', { type: 'warning' })
    await downloadApi.cancel(id)
    ElMessage.success('已删除')
    fetchDownloads()
  } catch {
    // 用户取消
  }
}

const handleDeleteFailed = async () => {
  try {
    await ElMessageBox.confirm('确定要删除全部失败任务及其本地临时文件吗？', '确认批量删除', { type: 'warning' })
    const { data } = await downloadApi.deleteFailed()
    ElMessage.success(`已删除 ${data.deletedCount} 个失败任务`)
    if (downloads.value.length === data.deletedCount && page.value > 1) {
      page.value--
    }
    fetchDownloads()
  } catch {
    // 用户取消
  }
}

const formatBytes = (bytes: number) => {
  if (bytes === 0) return '0 B'
  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

const formatSpeed = (bytesPerSec?: number) => {
  if (!bytesPerSec || bytesPerSec <= 0) return '-'
  const k = 1024
  const sizes = ['B/s', 'KB/s', 'MB/s', 'GB/s']
  const i = Math.floor(Math.log(bytesPerSec) / Math.log(k))
  return parseFloat((bytesPerSec / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

const formatTime = (dateStr?: string) => {
  if (!dateStr) return '-'
  return new Date(dateStr).toLocaleString('zh-CN')
}

const getProgress = (item: DownloadItem) => {
  if (item.fileSize === 0) return 0
  return Math.round((item.downloadedBytes / item.fileSize) * 100)
}

onMounted(() => {
  fetchDownloads()
  timer = setInterval(() => {
    fetchDownloads()
  }, 3000)
})

onUnmounted(() => {
  if (timer) clearInterval(timer)
})
</script>

<template>
  <div>
    <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px">
      <h2>下载队列</h2>
      <div style="display: flex; gap: 12px">
        <el-button type="danger" plain @click="handleDeleteFailed">批量删除失败任务</el-button>
        <el-select v-model="statusFilter" placeholder="状态筛选" clearable style="width: 150px" @change="fetchDownloads">
          <el-option label="全部" value="" />
          <el-option label="等待中" value="Pending" />
          <el-option label="下载中" value="Downloading" />
          <el-option label="已暂停" value="Paused" />
          <el-option label="已完成" value="Completed" />
          <el-option label="失败" value="Failed" />
        </el-select>
      </div>
    </div>

    <el-table
      :data="downloads"
      v-loading="loading"
      stripe
      style="width: 100%"
      height="calc(100vh - 160px)"
    >
      <el-table-column prop="fileName" label="文件名" min-width="200" show-overflow-tooltip />
      <el-table-column label="大小" width="100">
        <template #default="{ row }">{{ formatBytes(row.fileSize) }}</template>
      </el-table-column>
      <el-table-column label="进度" width="200">
        <template #default="{ row }">
          <el-progress
            :percentage="getProgress(row)"
            :status="row.status === 'Completed' ? 'success' : row.status === 'Failed' ? 'exception' : undefined"
          />
        </template>
      </el-table-column>
      <el-table-column label="已下载" width="100">
        <template #default="{ row }">{{ formatBytes(row.downloadedBytes) }}</template>
      </el-table-column>
      <el-table-column label="速度" width="100">
        <template #default="{ row }">
          <span :style="{ color: row.status === 'Downloading' && row.speedBytesPerSecond > 0 ? '#67c23a' : '' }">
            {{ formatSpeed(row.speedBytesPerSecond) }}
          </span>
        </template>
      </el-table-column>
      <el-table-column label="状态" width="100">
        <template #default="{ row }">
          <el-tag :type="(statusMap[row.status]?.type as any) || 'info'" size="small">
            {{ statusMap[row.status]?.label || row.status }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="重试" width="80">
        <template #default="{ row }">{{ row.retryCount ? `${row.retryCount}/3` : '-' }}</template>
      </el-table-column>
      <el-table-column prop="sourceType" label="来源" width="80" />
      <el-table-column label="创建时间" width="180">
        <template #default="{ row }">{{ formatTime(row.createdAt) }}</template>
      </el-table-column>
      <el-table-column label="操作" width="180" fixed="right">
        <template #default="{ row }">
          <el-button
            v-if="row.status === 'Downloading'"
            type="warning"
            size="small"
            @click="handlePause(row.id)"
          >暂停</el-button>
          <el-button
            v-if="row.status === 'Paused' || row.status === 'Pending'"
            type="primary"
            size="small"
            @click="handleResume(row.id)"
          >恢复</el-button>
          <el-button
            v-if="row.status === 'Failed'"
            type="primary"
            size="small"
            @click="handleRetry(row.id)"
          >重试</el-button>
          <el-button
            v-if="row.status !== 'Completed'"
            type="danger"
            size="small"
            @click="handleCancel(row.id)"
          >删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-pagination
      v-model:current-page="page"
      v-model:page-size="pageSize"
      :total="total"
      layout="total, prev, pager, next"
      style="margin-top: 20px; justify-content: flex-end"
      @current-change="fetchDownloads"
    />
  </div>
</template>
