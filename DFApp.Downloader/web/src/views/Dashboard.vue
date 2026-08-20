<script setup lang="ts">
import { ref, onMounted, onUnmounted, nextTick } from 'vue'
import { downloadApi, type GlobalStatus, type SpeedHistory, type SpeedHistoryRange } from '../api/downloader'
import { ElMessage } from 'element-plus'
import * as echarts from 'echarts'

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

const speedRange = ref<SpeedHistoryRange>('24h')
const chartLoading = ref(false)
const speedChartEl = ref<HTMLElement | null>(null)
let chartInstance: echarts.ECharts | null = null
let speedTimer: ReturnType<typeof setInterval> | null = null

const speedRangeOptions: { label: string; value: SpeedHistoryRange }[] = [
  { label: '最近1小时', value: '1h' },
  { label: '最近6小时', value: '6h' },
  { label: '最近24小时', value: '24h' },
  { label: '最近7天', value: '7d' },
  { label: '最近30天', value: '30d' },
]

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

const fetchSpeedHistory = async () => {
  chartLoading.value = true
  try {
    const { data } = await downloadApi.getSpeedHistory(speedRange.value)
    renderSpeedChart(data)
  } catch {
    // 静默处理
  } finally {
    chartLoading.value = false
  }
}

const renderSpeedChart = (history: SpeedHistory) => {
  if (!speedChartEl.value) return
  if (!chartInstance) {
    chartInstance = echarts.init(speedChartEl.value)
  }

  const pad = (n: number) => String(n).padStart(2, '0')
  const withDate = history.bucketSeconds > 900
  const labels = history.items.map((p) => {
    const d = new Date(p.time)
    const hm = `${pad(d.getHours())}:${pad(d.getMinutes())}`
    return withDate ? `${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${hm}` : hm
  })

  chartInstance.setOption({
    grid: { left: 12, right: 20, top: 36, bottom: 8, containLabel: true },
    tooltip: {
      trigger: 'axis',
      formatter: (params: unknown) => {
        const list = Array.isArray(params) ? params : [params]
        const idx = (list[0] as { dataIndex: number }).dataIndex
        const point = history.items[idx]
        if (!point) return ''
        return `${labels[idx]}<br/>平均速度：${formatSpeed(point.avgSpeed)}<br/>峰值速度：${formatSpeed(point.maxSpeed)}`
      },
    },
    legend: { data: ['平均速度', '峰值速度'], top: 0, right: 0, icon: 'plain' },
    xAxis: {
      type: 'category',
      boundaryGap: false,
      data: labels,
      axisLabel: { hideOverlap: true },
    },
    yAxis: {
      type: 'value',
      axisLabel: { formatter: (v: number) => formatSpeed(v) },
      splitLine: { lineStyle: { type: 'dashed' } },
    },
    series: [
      {
        name: '平均速度',
        type: 'line',
        smooth: true,
        symbol: 'none',
        data: history.items.map((p) => p.avgSpeed),
        itemStyle: { color: '#409eff' },
        lineStyle: { width: 2 },
        areaStyle: { opacity: 0.12 },
      },
      {
        name: '峰值速度',
        type: 'line',
        smooth: true,
        symbol: 'none',
        data: history.items.map((p) => p.maxSpeed),
        itemStyle: { color: '#e6a23c' },
        lineStyle: { width: 1.5, type: 'dashed' },
      },
    ],
  })
}

const handleResize = () => chartInstance?.resize()

onMounted(async () => {
  fetchStatus()
  timer = setInterval(fetchStatus, 3000)

  await nextTick()
  fetchSpeedHistory()
  speedTimer = setInterval(fetchSpeedHistory, 60000)
  window.addEventListener('resize', handleResize)
})

onUnmounted(() => {
  if (timer) clearInterval(timer)
  if (speedTimer) clearInterval(speedTimer)
  window.removeEventListener('resize', handleResize)
  chartInstance?.dispose()
  chartInstance = null
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

    <el-card shadow="hover" style="margin-top: 20px" v-loading="chartLoading">
      <template #header>
        <div class="card-header">
          <el-icon><TrendCharts /></el-icon>
          <span>速度记录</span>
          <el-radio-group v-model="speedRange" size="small" style="margin-left: auto" @change="fetchSpeedHistory">
            <el-radio-button v-for="opt in speedRangeOptions" :key="opt.value" :value="opt.value">
              {{ opt.label }}
            </el-radio-button>
          </el-radio-group>
        </div>
      </template>
      <div ref="speedChartEl" style="height: 320px"></div>
    </el-card>
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
