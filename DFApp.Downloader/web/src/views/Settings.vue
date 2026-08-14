<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { downloadApi, type DownloaderSettings } from '../api/downloader'
import { ElMessage } from 'element-plus'

const settings = ref<DownloaderSettings>({
  dfAppUrl: '',
  dfAppUsername: '',
  dfAppPassword: '',
  apacheUsername: '',
  apachePassword: '',
  downloadPath: '',
  maxConcurrentDownloads: 3,
  maxSegmentsPerFile: 4,
  segmentSize: 4194304,
  webServerPort: 9550,
  autoStart: false,
})
const loading = ref(false)
const syncing = ref(false)

const fetchSettings = async () => {
  loading.value = true
  try {
    const { data } = await downloadApi.getSettings()
    settings.value = data
  } catch {
    ElMessage.error('获取设置失败')
  } finally {
    loading.value = false
  }
}

const handleSave = async () => {
  try {
    await downloadApi.updateSettings(settings.value)
    ElMessage.success('设置已保存')
  } catch {
    ElMessage.error('保存失败')
  }
}

const handleSyncMissed = async () => {
  syncing.value = true
  try {
    const { data } = await downloadApi.syncMissed()
    if (data.added > 0 || data.reconciled > 0) {
      ElMessage.success(`扫描 ${data.scanned} 项，新增 ${data.added} 项到下载队列，修复回写 ${data.reconciled} 项`)
    } else {
      ElMessage.info(`扫描 ${data.scanned} 项，无遗漏`)
    }
  } catch {
    ElMessage.error('同步失败，请先确认已连接 DFApp 后端')
  } finally {
    syncing.value = false
  }
}

onMounted(fetchSettings)
</script>

<template>
  <div>
    <h2 style="margin-bottom: 20px">设置</h2>

    <el-form :model="settings" label-width="160px" v-loading="loading" style="max-width: 600px">
      <el-divider content-position="left">DFApp 连接</el-divider>

      <el-form-item label="DFApp 地址">
        <el-input v-model="settings.dfAppUrl" placeholder="http://localhost:44369" />
      </el-form-item>

      <el-form-item label="用户名">
        <el-input v-model="settings.dfAppUsername" placeholder="登录用户名" />
      </el-form-item>

      <el-form-item label="密码">
        <el-input v-model="settings.dfAppPassword" type="password" show-password placeholder="登录密码" />
      </el-form-item>

      <el-form-item label="遗漏同步">
        <el-button type="warning" :loading="syncing" @click="handleSyncMissed">
          同步遗漏下载
        </el-button>
        <span style="margin-left: 10px; color: #909399; font-size: 12px">拉取服务器已下载完成但本地缺失的媒体</span>
      </el-form-item>

      <el-divider content-position="left">Apache 下载服务器</el-divider>

      <el-form-item label="用户名">
        <el-input v-model="settings.apacheUsername" placeholder="Apache Basic Auth 用户名" />
      </el-form-item>

      <el-form-item label="密码">
        <el-input v-model="settings.apachePassword" type="password" show-password placeholder="Apache Basic Auth 密码" />
      </el-form-item>

      <el-divider content-position="left">下载配置</el-divider>

      <el-form-item label="保存路径">
        <el-input v-model="settings.downloadPath" placeholder="%USERPROFILE%\Downloads\DFApp" />
      </el-form-item>

      <el-form-item label="最大并发下载数">
        <el-input-number v-model="settings.maxConcurrentDownloads" :min="1" :max="10" />
      </el-form-item>

      <el-form-item label="每文件最大分片数">
        <el-input-number v-model="settings.maxSegmentsPerFile" :min="1" :max="16" />
      </el-form-item>

      <el-form-item label="分片大小 (MB)">
        <el-input-number :model-value="Math.round(settings.segmentSize / 1024 / 1024)" @update:model-value="(v: number) => settings.segmentSize = v * 1024 * 1024" :min="1" :max="64" />
      </el-form-item>

      <el-divider content-position="left">其他</el-divider>

      <el-form-item label="Web 端口">
        <el-input-number v-model="settings.webServerPort" :min="1024" :max="65535" />
      </el-form-item>

      <el-form-item label="开机自启">
        <el-switch v-model="settings.autoStart" />
      </el-form-item>

      <el-form-item>
        <el-button type="primary" @click="handleSave" :loading="loading">保存设置</el-button>
      </el-form-item>
    </el-form>
  </div>
</template>
