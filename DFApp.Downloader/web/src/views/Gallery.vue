<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { downloadApi } from '../api/downloader'
import { ElMessage, ElImageViewer } from 'element-plus'

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
  thumbUrl?: string | null
}

const items = ref<GalleryItem[]>([])
const loading = ref(false)
const filter = ref('image') // image | video | all
const columnCount = ref(3)
const showBackToTop = ref(false)
let timer: ReturnType<typeof setInterval> | null = null

// 大图预览状态
const viewerVisible = ref(false)
const viewerIndex = ref(0)
const previewList = computed(() =>
  filteredItems().filter(isImage).map(i => i.mediaUrl),
)

const isVideo = (item: GalleryItem) =>
  item.mimeType?.toLowerCase().startsWith('video') || /\.(mp4|mkv|avi|mov|webm)$/i.test(item.fileName)

const isImage = (item: GalleryItem) =>
  item.mimeType?.toLowerCase().startsWith('image') || /\.(jpg|jpeg|png|gif|webp|bmp)$/i.test(item.fileName)

const filteredItems = () => {
  if (filter.value === 'image') return items.value.filter(isImage)
  if (filter.value === 'video') return items.value.filter(isVideo)
  return items.value
}

// 瀑布流分列：按顺序轮流放入各列，保持从左到右的阅读顺序
const columns = computed(() => {
  const list = filteredItems()
  const cols: GalleryItem[][] = Array.from({ length: columnCount.value }, () => [])
  list.forEach((item, i) => cols[i % columnCount.value].push(item))
  return cols
})

// 按窗口宽度调整列数（响应式），图片列宽随之变大
const updateColumns = () => {
  const w = window.innerWidth
  columnCount.value = w >= 1800 ? 4 : w >= 1200 ? 3 : w >= 700 ? 2 : 1
}

const openPreview = (item: GalleryItem) => {
  const idx = previewList.value.indexOf(item.mediaUrl)
  if (idx >= 0) {
    viewerIndex.value = idx
    viewerVisible.value = true
  }
}

const fetchGallery = async () => {
  loading.value = true
  try {
    // 瀑布流一次性加载全部已完成媒体（图片懒加载，量级可接受）
    const { data } = await downloadApi.getGallery(1, 10000)
    items.value = data.items
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

// 页面滚动超过一屏时显示"回到顶部"，点击平滑滚回顶部
const onScroll = () => {
  showBackToTop.value = window.scrollY > window.innerHeight
}

const scrollToTop = () => {
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

onMounted(() => {
  updateColumns()
  window.addEventListener('resize', updateColumns)
  window.addEventListener('scroll', onScroll)
  fetchGallery()
  timer = setInterval(fetchGallery, 30000)
})

onUnmounted(() => {
  window.removeEventListener('resize', updateColumns)
  window.removeEventListener('scroll', onScroll)
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

    <div v-loading="loading" style="display: flex; gap: 16px; align-items: flex-start">
      <!-- 瀑布流：每列独立纵向排列，图片高度自适应（不再裁剪），宽度随列数自动加大 -->
      <div
        v-for="(col, ci) in columns"
        :key="ci"
        style="flex: 1; min-width: 0; display: flex; flex-direction: column; gap: 16px"
      >
        <el-card
          v-for="item in col"
          :key="item.id"
          shadow="hover"
          :body-style="{ padding: '10px' }"
          style="break-inside: avoid"
        >
          <!-- 图片：自适应高度显示原始比例，点击放大 -->
          <img
            v-if="isImage(item)"
            :src="item.mediaUrl"
            :alt="item.fileName"
            loading="lazy"
            style="width: 100%; display: block; border-radius: 6px; cursor: zoom-in"
            @click="openPreview(item)"
          />
          <!-- 视频：缩略图（16:9 裁切）+ 点击 VLC 播放（无缩略图时显示占位） -->
          <div
            v-else-if="isVideo(item)"
            style="width: 100%; border-radius: 6px; background: #0d1117; display: flex; align-items: center; justify-content: center; color: #fff; flex-direction: column; gap: 12px; overflow: hidden; position: relative; cursor: pointer; aspect-ratio: 16 / 9"
            @click="playWithVlc(item)"
          >
            <el-image
              v-if="item.thumbUrl"
              :src="item.thumbUrl"
              fit="cover"
              style="width: 100%; height: 100%"
            />
            <span v-else style="font-size: 40px">🎬</span>
            <span
              style="position: absolute; bottom: 8px; left: 50%; transform: translateX(-50%); background: rgba(0,0,0,0.6); padding: 4px 12px; border-radius: 4px; font-size: 12px"
            >▶ 用 VLC 播放</span>
          </div>
          <div v-else style="width: 100%; aspect-ratio: 16 / 9; border-radius: 6px; background: #f0f2f5; display: flex; align-items: center; justify-content: center; color: #909399">
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
    </div>

    <!-- 回到顶部（滚动超过一屏时显示） -->
    <el-button
      v-if="showBackToTop"
      type="primary"
      circle
      style="position: fixed; right: 30px; bottom: 40px; z-index: 10"
      title="回到顶部"
      @click="scrollToTop"
    >
      <el-icon><Top /></el-icon>
    </el-button>

    <!-- 大图预览（el-image-viewer 全屏查看） -->
    <el-image-viewer
      v-if="viewerVisible"
      :url-list="previewList"
      :initial-index="viewerIndex"
      @close="viewerVisible = false"
    />
  </div>
</template>
