<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { downloadApi, type LogFileInfo, type LogContent, type LogOrder } from '../api/downloader'
import { ElMessage } from 'element-plus'

const files = ref<LogFileInfo[]>([])
const currentFile = ref<string>('')
const content = ref<LogContent | null>(null)
const lines = ref(800)
const order = ref<LogOrder>('tail')
const autoRefresh = ref(true)
const loading = ref(false)

let timer: ReturnType<typeof setInterval> | null = null

const fetchFiles = async () => {
  try {
    const { data } = await downloadApi.getLogList()
    files.value = data.items
    // 默认选中最新文件
    if (!currentFile.value && files.value.length > 0) {
      currentFile.value = files.value[0].fileName
      await fetchContent()
    }
  } catch {
    ElMessage.error('获取日志列表失败')
  }
}

const fetchContent = async () => {
  if (!currentFile.value) return
  loading.value = true
  try {
    const { data } = await downloadApi.getLogContent(currentFile.value, lines.value, order.value)
    content.value = data
  } catch {
    ElMessage.error('读取日志内容失败')
  } finally {
    loading.value = false
  }
}

const handleSelect = (fileName: string) => {
  currentFile.value = fileName
  fetchContent()
}

const handleRefresh = async () => {
  await fetchFiles()
  await fetchContent()
}

// 按日志级别着色，转义后用 span 包裹；倒序显示（最新在最上面）
const colorizedHtml = computed(() => {
  if (!content.value) return ''
  return content.value.content
    .split('\n')
    .reverse()
    .map((line) => {
      const escaped = line
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
      const m = escaped.match(/\[\d{2}:\d{2}:\d{2}\s+(INF|WRN|ERR|FTL|DBG|VRB)\]/)
      if (!m) return escaped
      const level = m[1]
      const cls =
        level === 'ERR' || level === 'FTL'
          ? 'log-err'
          : level === 'WRN'
            ? 'log-warn'
            : level === 'INF'
              ? 'log-info'
              : 'log-default'
      return `<span class="${cls}">${escaped}</span>`
    })
    .join('\n')
})

const formatSize = (bytes: number) => {
  if (bytes < 1024) return bytes + ' B'
  if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB'
  return (bytes / 1024 / 1024).toFixed(2) + ' MB'
}

const startTimer = () => {
  stopTimer()
  timer = setInterval(fetchContent, 5000)
}

const stopTimer = () => {
  if (timer) {
    clearInterval(timer)
    timer = null
  }
}

// 行数或方向变化时重新加载
watch([lines, order], () => {
  fetchContent()
})

// 自动刷新仅在 tail 模式生效
watch(order, (val) => {
  if (val !== 'tail') {
    autoRefresh.value = false
    stopTimer()
  }
})

watch(autoRefresh, (val) => {
  if (val && order.value === 'tail') {
    startTimer()
  } else {
    stopTimer()
  }
})

onMounted(() => {
  fetchFiles()
  if (autoRefresh.value && order.value === 'tail') {
    startTimer()
  }
})

onUnmounted(() => {
  stopTimer()
})
</script>

<template>
  <div>
    <h2 style="margin-bottom: 20px">日志查看</h2>

    <el-row :gutter="20">
      <el-col :span="6">
        <el-card shadow="hover" v-loading="loading">
          <template #header>
            <div class="card-header">
              <span>日志文件</span>
              <el-button size="small" :icon="'Refresh'" @click="handleRefresh" circle />
            </div>
          </template>
          <div
            v-for="f in files"
            :key="f.fileName"
            class="log-file-item"
            :class="{ active: f.fileName === currentFile }"
            @click="handleSelect(f.fileName)"
          >
            <div class="file-name" :title="f.fileName">{{ f.fileName }}</div>
            <div class="file-meta">
              {{ formatSize(f.sizeBytes) }} · {{ f.lastWriteTime }}
            </div>
          </div>
          <el-empty v-if="files.length === 0" description="暂无日志文件" :image-size="60" />
        </el-card>
      </el-col>

      <el-col :span="18">
        <el-card shadow="hover" v-loading="loading">
          <template #header>
            <div class="card-header">
              <span>{{ currentFile || '请选择日志文件' }}</span>
              <div class="toolbar">
                <el-radio-group v-model="order" size="small">
                  <el-radio-button label="tail">末尾</el-radio-button>
                  <el-radio-button label="head">开头</el-radio-button>
                </el-radio-group>
                <el-select v-model="lines" size="small" style="width: 110px">
                  <el-option :value="200" label="200 行" />
                  <el-option :value="500" label="500 行" />
                  <el-option :value="800" label="800 行" />
                  <el-option :value="1000" label="1000 行" />
                  <el-option :value="2000" label="2000 行" />
                </el-select>
                <el-tooltip content="自动刷新（仅末尾模式，每 5 秒）" placement="top">
                  <el-switch v-model="autoRefresh" />
                </el-tooltip>
              </div>
            </div>
          </template>

          <div v-if="content" class="log-meta">
            共 {{ content.totalLines }} 行，显示 {{ content.returnedLines }} 行（{{ content.order === 'tail' ? '末尾' : '开头' }}）· 最新在前
          </div>
          <pre v-if="content" class="log-content" v-html="colorizedHtml"></pre>
          <el-empty v-else description="请选择左侧日志文件" :image-size="80" />
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<style scoped>
.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  font-size: 14px;
}

.toolbar {
  display: flex;
  align-items: center;
  gap: 10px;
}

.log-file-item {
  padding: 8px 10px;
  border-radius: 4px;
  cursor: pointer;
  transition: background-color 0.2s;
}

.log-file-item:hover {
  background-color: #f5f7fa;
}

.log-file-item.active {
  background-color: #ecf5ff;
  color: #409eff;
}

.file-name {
  font-size: 13px;
  word-break: break-all;
}

.file-meta {
  font-size: 12px;
  color: #909399;
  margin-top: 2px;
}

.log-meta {
  font-size: 12px;
  color: #909399;
  margin-bottom: 8px;
}

.log-content {
  background-color: #1e1e1e;
  color: #d4d4d4;
  padding: 12px;
  border-radius: 4px;
  font-size: 12px;
  line-height: 1.6;
  max-height: 65vh;
  overflow: auto;
  white-space: pre-wrap;
  word-break: break-all;
  margin: 0;
}

:deep(.log-err) {
  color: #f56c6c;
}

:deep(.log-warn) {
  color: #e6a23c;
}

:deep(.log-info) {
  color: #67c23a;
}

:deep(.log-default) {
  color: #d4d4d4;
}
</style>
