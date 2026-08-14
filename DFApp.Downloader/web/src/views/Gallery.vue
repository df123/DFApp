<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { downloadApi } from '../api/downloader'
import { ElMessage } from 'element-plus'

interface GalleryItem {
  id: number
  fileName: string
  fileSize: number
  mimeType: string
  chatTitle: string
  message?: string | null
  completedAt: string
  mediaUrl: string
  windowsPath: string
}

const items = ref<GalleryItem[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(60)
const loading = ref(false)
const filter = ref('image') // image | video | all
let timer: ReturnType<typeof setInterval> | null = null

const isVideo = (item: GalleryItem) =>
  item.mimeType?.toLowerCase().startsWith('video') || /\.(mp4|mkv|avi|mov|webm)$/i.test(item.fileName)

const isImage = (item: GalleryItem) =>
  item.mimeType?.toLowerCase().startsWith('image') || /\.(jpg|jpeg|png|gif|webp|bmp)$/i.test(item.fileName)

const filteredItems = () => {
  if (filter.value === 'image') return items.value.filter(isImage)
  if (filter.value === 'video') return items.value.filter(isVideo)
  return items.value
}

const fetchGallery = async () => {
  loading.value = true
  try {
    const { data } = await downloadApi.getGallery(page.value, pageSize.value)
    items.value = data.items
    total.value = data.total
  } catch {
    ElMessage.error('获取媒体库失败')
  } finally {
    loading.value = false
  }
}

// 通过注册的 vlc: 协议直接唤起 Windows 侧 VLC 播放本地文件
// 注意不能用 vlc://（双斜杠）：浏览器会按带主机名解析，把 D: 当 authority 吃掉盘符冒号；
// vlc: 是 opaque path，浏览器原样传递
const playWithVlc = (item: GalleryItem) => {
  window.location.href = `vlc:${item.windowsPath}`
}

const formatBytes = (bytes: number) => {
  if (bytes === 0) return '0 B'
  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

onMounted(() => {
  fetchGallery()
  timer = setInterval(fetchGallery, 30000)
})

onUnmounted(() => {
  if (timer) clearInterval(timer)
})
</script>

<template>
  <div>
    <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px">
      <h2>媒体库</h2>
      <el-radio-group v-model="filter" @change="fetchGallery">
        <el-radio-button value="image">图片</el-radio-button>
        <el-radio-button value="video">视频</el-radio-button>
        <el-radio-button value="all">全部</el-radio-button>
      </el-radio-group>
    </div>

    <el-empty v-if="!loading && filteredItems().length === 0" description="暂无媒体" />

    <div v-loading="loading" style="display: grid; grid-template-columns: repeat(auto-fill, minmax(260px, 1fr)); gap: 16px">
      <el-card v-for="item in filteredItems()" :key="item.id" shadow="hover" :body-style="{ padding: '10px' }">
        <!-- 图片：点击放大 -->
        <el-image
          v-if="isImage(item)"
          :src="item.mediaUrl"
          :preview-src-list="filteredItems().filter(isImage).map(i => i.mediaUrl)"
          :initial-index="filteredItems().filter(isImage).findIndex(i => i.id === item.id)"
          fit="cover"
          style="width: 100%; height: 220px; border-radius: 6px; cursor: zoom-in"
        />
        <!-- 视频：封面占位 + VLC 播放按钮 -->
        <div
          v-else-if="isVideo(item)"
          style="width: 100%; height: 220px; border-radius: 6px; background: #0d1117; display: flex; align-items: center; justify-content: center; color: #fff; flex-direction: column; gap: 12px"
        >
          <span style="font-size: 40px">🎬</span>
          <el-button type="primary" size="small" @click="playWithVlc(item)">
            用 VLC 播放
          </el-button>
        </div>
        <div v-else style="width: 100%; height: 220px; border-radius: 6px; background: #f0f2f5; display: flex; align-items: center; justify-content: center; color: #909399">
          {{ item.mimeType || '未知类型' }}
        </div>

        <div style="margin-top: 8px">
          <div style="font-weight: 600; font-size: 13px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap">
            {{ item.chatTitle || '（无标题）' }}
          </div>
          <div
            v-if="item.message"
            style="font-size: 12px; color: #606266; margin-top: 4px; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden"
          >
            {{ item.message }}
          </div>
          <div style="font-size: 12px; color: #909399; margin-top: 4px; display: flex; justify-content: space-between">
            <span>{{ formatBytes(item.fileSize) }}</span>
            <span>{{ item.fileName }}</span>
          </div>
        </div>
      </el-card>
    </div>

    <el-pagination
      v-model:current-page="page"
      v-model:page-size="pageSize"
      :total="total"
      layout="total, prev, pager, next"
      style="margin-top: 20px; justify-content: flex-end"
      @current-change="fetchGallery"
    />
  </div>
</template>
